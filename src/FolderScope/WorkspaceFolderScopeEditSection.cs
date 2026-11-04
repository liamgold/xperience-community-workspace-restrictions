using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using XperienceCommunity.WorkspaceRestrictions;

[assembly: UIPage(typeof(WorkspaceFolderScopeListingPage), PageParameterConstants.PARAMETERIZED_SLUG,
    typeof(WorkspaceFolderScopeEditSection), "Folder scope", TemplateNames.SECTION_LAYOUT, UIPageOrder.NoOrder)]

namespace XperienceCommunity.WorkspaceRestrictions;

public class WorkspaceFolderScopeEditSection : EditSectionPage<WorkspaceFolderScopeInfo>
{
    [PageParameter(typeof(IntPageModelBinder), typeof(WorkspaceEditSection))]
    public int WorkspaceId { get; set; }

    protected override string ObjectType => WorkspaceFolderScopeInfo.OBJECT_TYPE;

    public override async Task<PageValidationResult> ValidatePage()
    {
        var result = await base.ValidatePage();
        if (!result.IsValid)
        {
            return result;
        }

        var scope = await WorkspaceFolderScopeInfo.Provider.GetAsync(ObjectId);
        var folder = scope == null ? null : await ContentFolderInfo.Provider.GetAsync(scope.WorkspaceFolderScopeFolderID);
        return new PageValidationResult
        {
            IsValid = scope?.WorkspaceFolderScopeWorkspaceID == WorkspaceId && folder?.ContentFolderWorkspaceID == WorkspaceId,
            ErrorMessageKey = "The folder scope does not belong to this workspace.",
        };
    }

    protected override async Task<string> GetObjectDisplayName(BaseInfo infoObject)
    {
        var scope = (WorkspaceFolderScopeInfo)infoObject;
        var folder = await ContentFolderInfo.Provider.GetAsync(scope.WorkspaceFolderScopeFolderID);
        return folder?.ContentFolderDisplayName ?? "Folder scope";
    }
}
