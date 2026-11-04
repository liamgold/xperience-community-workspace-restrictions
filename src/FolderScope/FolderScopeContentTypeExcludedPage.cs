using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using XperienceCommunity.WorkspaceRestrictions;

[assembly: UIPage(typeof(WorkspaceFolderScopeEditSection), "excluded-content-types", typeof(FolderScopeContentTypeExcludedPage),
    "Excluded content types", TemplateNames.BINDING, UIPageOrder.First + 2)]

namespace XperienceCommunity.WorkspaceRestrictions;

public class FolderScopeContentTypeExcludedPage(
    IInfoProvider<WorkspaceContentTypeAllowedInfo> allowedProvider,
    IInfoProvider<WorkspaceContentTypeExcludedInfo> excludedProvider)
    : FolderScopeContentTypeBindingPage<FolderScopeContentTypeExcludedInfo>(allowedProvider, excludedProvider)
{
    protected override string Caption => "Excluded content types";

    protected override string SourceBindingColumn => nameof(FolderScopeContentTypeExcludedInfo.FolderScopeContentTypeExcludedFolderScopeID);

    protected override string TargetBindingColumn => nameof(FolderScopeContentTypeExcludedInfo.FolderScopeContentTypeExcludedClassID);
}
