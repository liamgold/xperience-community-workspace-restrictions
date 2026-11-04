using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.UIPages;
using XperienceCommunity.WorkspaceRestrictions;

[assembly: PageExtender(typeof(WorkspaceContentTypeFilterExtender))]

namespace XperienceCommunity.WorkspaceRestrictions;

public class WorkspaceContentTypeFilterExtender : PageExtender<ContentItemCreate>
{
    private readonly IInfoProvider<WorkspaceContentTypeAllowedInfo> allowedProvider;
    private readonly IInfoProvider<WorkspaceContentTypeExcludedInfo> excludedProvider;
    private readonly IInfoProvider<WorkspaceFolderScopeInfo> scopeProvider;
    private readonly IInfoProvider<FolderScopeContentTypeAllowedInfo> folderAllowedProvider;
    private readonly IInfoProvider<FolderScopeContentTypeExcludedInfo> folderExcludedProvider;
    private readonly IInfoProvider<ContentFolderInfo> folderProvider;

    public WorkspaceContentTypeFilterExtender(
        IInfoProvider<WorkspaceContentTypeAllowedInfo> allowedProvider,
        IInfoProvider<WorkspaceContentTypeExcludedInfo> excludedProvider,
        IInfoProvider<WorkspaceFolderScopeInfo> scopeProvider,
        IInfoProvider<FolderScopeContentTypeAllowedInfo> folderAllowedProvider,
        IInfoProvider<FolderScopeContentTypeExcludedInfo> folderExcludedProvider,
        IInfoProvider<ContentFolderInfo> folderProvider)
    {
        this.allowedProvider = allowedProvider;
        this.excludedProvider = excludedProvider;
        this.scopeProvider = scopeProvider;
        this.folderAllowedProvider = folderAllowedProvider;
        this.folderExcludedProvider = folderExcludedProvider;
        this.folderProvider = folderProvider;
    }

    public override async Task<TemplateClientProperties> ConfigureTemplateProperties(TemplateClientProperties properties)
    {
        if (properties is not ContentItemClientProperties contentItemProps)
        {
            return properties;
        }

        var workspaceId = Page.WorkspaceIdentifier?.WorkspaceID;
        if (workspaceId is null or 0)
        {
            return properties;
        }

        if (contentItemProps.Items.OfType<TileSelectorClientProperties>().FirstOrDefault() is not { } tileProps)
        {
            return properties;
        }

        var workspaceRestriction = await ContentTypeRestriction.ForWorkspace(workspaceId.Value, allowedProvider, excludedProvider);
        var folderRestrictions = await GetFolderRestrictions(workspaceId.Value);
        FilterContentTypes(tileProps, workspaceRestriction, folderRestrictions);

        return properties;
    }

    internal static void FilterContentTypes(TileSelectorClientProperties properties,
        ContentTypeRestriction workspaceRestriction, IReadOnlyCollection<ContentTypeRestriction> folderRestrictions)
    {
        var items = properties.Items.Where(tile => workspaceRestriction.Allows(tile.Identifier, folderRestrictions)).ToArray();
        properties.Items = items;

        if (properties.Value != null && !items.Any(tile => tile.Identifier == properties.Value.Identifier))
        {
            properties.Value = null;
        }
    }

    private async Task<IReadOnlyCollection<ContentTypeRestriction>> GetFolderRestrictions(int workspaceId)
    {
        // Smart folders have their own IDs and must not match a content folder scope.
        if (Page.FolderId.FolderType != FolderType.Content || Page.FolderId.Id is not > 0)
        {
            return [];
        }

        var folder = await folderProvider.GetAsync(Page.FolderId.Id.Value);
        if (folder == null || folder.ContentFolderWorkspaceID != workspaceId)
        {
            return [];
        }

        var scopes = await scopeProvider.Get()
            .WhereEquals(nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeWorkspaceID), workspaceId)
            .GetEnumerableTypedResultAsync();
        var scopeIds = await FolderScopeResolver.GetApplicableScopeIds(folder, scopes, async id => await folderProvider.GetAsync(id));

        if (scopeIds.Length == 0)
        {
            return [];
        }

        var allowedBindings = (await folderAllowedProvider.Get()
            .WhereIn(nameof(FolderScopeContentTypeAllowedInfo.FolderScopeContentTypeAllowedFolderScopeID), scopeIds)
            .GetEnumerableTypedResultAsync())
            .ToLookup(binding => binding.FolderScopeContentTypeAllowedFolderScopeID, binding => binding.FolderScopeContentTypeAllowedClassID);
        var excludedBindings = (await folderExcludedProvider.Get()
            .WhereIn(nameof(FolderScopeContentTypeExcludedInfo.FolderScopeContentTypeExcludedFolderScopeID), scopeIds)
            .GetEnumerableTypedResultAsync())
            .ToLookup(binding => binding.FolderScopeContentTypeExcludedFolderScopeID, binding => binding.FolderScopeContentTypeExcludedClassID);

        // Keep each scope separate so its allow-list precedence cannot override another scope's restrictions.
        return scopeIds.Select(id => new ContentTypeRestriction(allowedBindings[id].ToHashSet(), excludedBindings[id].ToHashSet())).ToArray();
    }
}
