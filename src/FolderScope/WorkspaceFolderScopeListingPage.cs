using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using XperienceCommunity.WorkspaceRestrictions;

[assembly: UIPage(typeof(WorkspaceEditSection), WorkspaceFolderScopeListingPage.SLUG,
    typeof(WorkspaceFolderScopeListingPage), "Folder scopes", TemplateNames.LISTING, UIPageOrder.NoOrder)]

namespace XperienceCommunity.WorkspaceRestrictions;

[UINavigation(false)]
public class WorkspaceFolderScopeListingPage : ListingPage
{
    internal const string SLUG = "folder-scopes";

    protected override string ObjectType => WorkspaceFolderScopeInfo.OBJECT_TYPE;

    [PageParameter(typeof(IntPageModelBinder), typeof(WorkspaceEditSection))]
    public int WorkspaceId { get; set; }

    public override async Task ConfigurePage()
    {
        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(ContentFolderInfo.ContentFolderDisplayName), caption: "Folder", searchable: true)
            .AddColumn(nameof(ContentFolderInfo.ContentFolderTreePath), caption: "Path", searchable: true, defaultSortDirection: SortTypeEnum.Asc)
            .AddColumn(nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeApplyToChildFolders), caption: "Apply to child folders");

        PageConfiguration.QueryModifiers.AddModifier(query => query
            .Source(source => source.InnerJoin<ContentFolderInfo>(
                nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeFolderID), nameof(ContentFolderInfo.ContentFolderID)))
            .WhereEquals(nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeWorkspaceID), WorkspaceId)
            .WhereEquals(nameof(ContentFolderInfo.ContentFolderWorkspaceID), WorkspaceId));

        var parameters = new PageParameterValues
        {
            { typeof(WorkspaceEditSection), WorkspaceId },
        };

        PageConfiguration.HeaderActions.AddLink<WorkspaceFolderScopeCreatePage>("New folder scope", parameters: parameters);
        PageConfiguration.AddEditRowAction<WorkspaceFolderScopeSettingsPage>(parameters: parameters);
        PageConfiguration.TableActions.AddDeleteAction(nameof(Delete));

        await base.ConfigurePage();
    }

    [PageCommand(Permission = SystemPermissions.DELETE)]
    public override async Task<ICommandResponse<RowActionResult>> Delete(int id)
    {
        var scope = await WorkspaceFolderScopeInfo.Provider.GetAsync(id);
        if (scope == null || scope.WorkspaceFolderScopeWorkspaceID != WorkspaceId)
        {
            return ResponseFrom(new RowActionResult(false)).AddErrorMessage("The folder scope does not belong to this workspace.");
        }

        return await base.Delete(id);
    }
}
