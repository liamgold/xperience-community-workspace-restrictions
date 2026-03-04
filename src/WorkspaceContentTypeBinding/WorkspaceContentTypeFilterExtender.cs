using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.UIPages;
using XperienceCommunity.WorkspaceRestrictions;

[assembly: PageExtender(typeof(WorkspaceContentTypeFilterExtender))]

namespace XperienceCommunity.WorkspaceRestrictions;

public class WorkspaceContentTypeFilterExtender : PageExtender<ContentItemCreate>
{
    private readonly IInfoProvider<WorkspaceContentTypeBindingInfo> bindingProvider;
    private readonly IInfoProvider<WorkspaceContentTypeExclusionInfo> exclusionProvider;

    public WorkspaceContentTypeFilterExtender(
        IInfoProvider<WorkspaceContentTypeBindingInfo> bindingProvider,
        IInfoProvider<WorkspaceContentTypeExclusionInfo> exclusionProvider)
    {
        this.bindingProvider = bindingProvider;
        this.exclusionProvider = exclusionProvider;
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

        var allowedClassIds = (await bindingProvider.Get()
            .WhereEquals(nameof(WorkspaceContentTypeBindingInfo.WorkspaceContentTypeBindingWorkspaceID), workspaceId)
            .GetEnumerableTypedResultAsync())
            .Select(b => b.WorkspaceContentTypeBindingClassID)
            .ToHashSet();

        var excludedClassIds = (await exclusionProvider.Get()
            .WhereEquals(nameof(WorkspaceContentTypeExclusionInfo.WorkspaceContentTypeExclusionWorkspaceID), workspaceId)
            .GetEnumerableTypedResultAsync())
            .Select(b => b.WorkspaceContentTypeExclusionClassID)
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
