using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using XperienceCommunity.WorkspaceRestrictions;

[assembly: UIPage(
    typeof(WorkspaceEditSection),
    WorkspaceContentTypeAllowedPage.SLUG,
    typeof(WorkspaceContentTypeAllowedPage),
    "Allowed content types",
    TemplateNames.BINDING,
    UIPageOrder.NoOrder)]

namespace XperienceCommunity.WorkspaceRestrictions;

[UINavigation(false)]
public class WorkspaceContentTypeAllowedPage : InfoBindingPage<WorkspaceContentTypeAllowedInfo, DataClassInfo>
{
    internal const string SLUG = "allowed-content-types";

    [PageParameter(typeof(IntPageModelBinder), typeof(WorkspaceEditSection))]
    public override int EditedObjectId { get; set; }

    protected override string SourceBindingColumn => nameof(WorkspaceContentTypeAllowedInfo.WorkspaceContentTypeAllowedWorkspaceID);

    protected override string TargetBindingColumn => nameof(WorkspaceContentTypeAllowedInfo.WorkspaceContentTypeAllowedClassID);

    public override async Task ConfigurePage()
    {
        PageConfiguration.ExistingBindingsListing.Caption = "Allowed content types";
        PageConfiguration.BindingSidePanelListing.Caption = "Available content types";
        PageConfiguration.AddBindingButtonText = "Add content type";
        PageConfiguration.SaveBindingsButtonText = "Save";

        PageConfiguration.ExistingBindingsListing.ColumnConfigurations
            .AddColumn(nameof(DataClassInfo.ClassDisplayName), caption: "Display name", searchable: true, defaultSortDirection: SortTypeEnum.Asc);

        PageConfiguration.BindingSidePanelListing.ColumnConfigurations
            .AddColumn(nameof(DataClassInfo.ClassDisplayName), caption: "Display name", searchable: true, defaultSortDirection: SortTypeEnum.Asc);

        PageConfiguration.BindingSidePanelListing.QueryModifiers
            .AddModifier(query => query.WhereEquals(nameof(DataClassInfo.ClassContentTypeType), ClassContentTypeType.REUSABLE));

        await base.ConfigurePage();
    }
}
