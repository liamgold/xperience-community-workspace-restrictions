using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using XperienceCommunity.WorkspaceRestrictions;

[assembly: PageExtender(typeof(WorkspaceEditSectionExtender))]

namespace XperienceCommunity.WorkspaceRestrictions;

public class WorkspaceEditSectionExtender : PageExtender<WorkspaceEditSection>
{
    public override async Task<TemplateClientProperties> ConfigureTemplateProperties(TemplateClientProperties properties)
    {
        properties = await base.ConfigureTemplateProperties(properties);

        var items = properties.Navigation.Items.ToList();
        items.Add(new NavigationItem
        {
            Label = "Allowed content types",
            Path = WorkspaceContentTypeBindingPage.SLUG,
        });
        items.Add(new NavigationItem
        {
            Label = "Excluded content types",
            Path = WorkspaceContentTypeExclusionPage.SLUG,
        });

        properties.Navigation.Items = items;

        return properties;
    }
}
