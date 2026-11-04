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
    private readonly ContentTypeRestrictionResolver resolver;

    public WorkspaceContentTypeFilterExtender(
        IInfoProvider<WorkspaceContentTypeAllowedInfo> allowedProvider,
        IInfoProvider<WorkspaceContentTypeExcludedInfo> excludedProvider,
        IInfoProvider<WorkspaceFolderScopeInfo> scopeProvider,
        IInfoProvider<FolderScopeContentTypeAllowedInfo> folderAllowedProvider,
        IInfoProvider<FolderScopeContentTypeExcludedInfo> folderExcludedProvider,
        IInfoProvider<ContentFolderInfo> folderProvider)
    {
        resolver = new ContentTypeRestrictionResolver(allowedProvider, excludedProvider, scopeProvider,
            folderAllowedProvider, folderExcludedProvider, folderProvider);
    }

    public override Task<TemplateClientProperties> ConfigureTemplateProperties(TemplateClientProperties properties)
    {
        if (properties is not ContentItemClientProperties contentItemProps)
        {
            return Task.FromResult(properties);
        }

        var workspaceId = Page.WorkspaceIdentifier?.WorkspaceID;
        if (workspaceId is null or 0)
        {
            return Task.FromResult(properties);
        }

        if (contentItemProps.Items.OfType<TileSelectorClientProperties>().FirstOrDefault() is not { } tileProps)
        {
            return Task.FromResult(properties);
        }

        // Smart folders have their own IDs and must not match a content folder scope.
        var folderId = Page.FolderId.FolderType == FolderType.Content ? Page.FolderId.Id ?? 0 : 0;

        FilterContentTypes(tileProps, resolver.GetWorkspaceRestriction(workspaceId.Value),
            resolver.GetFolderRestrictions(workspaceId.Value, folderId));

        return Task.FromResult(properties);
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
}
