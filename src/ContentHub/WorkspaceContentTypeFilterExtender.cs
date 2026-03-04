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

    public WorkspaceContentTypeFilterExtender(
        IInfoProvider<WorkspaceContentTypeAllowedInfo> allowedProvider,
        IInfoProvider<WorkspaceContentTypeExcludedInfo> excludedProvider)
    {
        this.allowedProvider = allowedProvider;
        this.excludedProvider = excludedProvider;
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

        var allowedClassIds = (await allowedProvider.Get()
            .WhereEquals(nameof(WorkspaceContentTypeAllowedInfo.WorkspaceContentTypeAllowedWorkspaceID), workspaceId)
            .GetEnumerableTypedResultAsync())
            .Select(b => b.WorkspaceContentTypeAllowedClassID)
            .ToHashSet();

        var excludedClassIds = (await excludedProvider.Get()
            .WhereEquals(nameof(WorkspaceContentTypeExcludedInfo.WorkspaceContentTypeExcludedWorkspaceID), workspaceId)
            .GetEnumerableTypedResultAsync())
            .Select(b => b.WorkspaceContentTypeExcludedClassID)
            .ToHashSet();

        if (allowedClassIds.Count == 0 && excludedClassIds.Count == 0)
        {
            return properties;
        }

        if (contentItemProps.Items.OfType<TileSelectorClientProperties>().FirstOrDefault() is { } tileProps)
        {
            tileProps.Items = allowedClassIds.Count > 0
                ? tileProps.Items.Where(t => allowedClassIds.Contains(t.Identifier))
                : tileProps.Items.Where(t => !excludedClassIds.Contains(t.Identifier));
        }

        return properties;
    }
}
