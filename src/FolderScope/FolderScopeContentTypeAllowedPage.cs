using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using XperienceCommunity.WorkspaceRestrictions;

[assembly: UIPage(typeof(WorkspaceFolderScopeEditSection), "allowed-content-types", typeof(FolderScopeContentTypeAllowedPage),
    "Allowed content types", TemplateNames.BINDING, UIPageOrder.First + 1)]

namespace XperienceCommunity.WorkspaceRestrictions;

public class FolderScopeContentTypeAllowedPage(
    IInfoProvider<WorkspaceContentTypeAllowedInfo> allowedProvider,
    IInfoProvider<WorkspaceContentTypeExcludedInfo> excludedProvider)
    : FolderScopeContentTypeBindingPage<FolderScopeContentTypeAllowedInfo>(allowedProvider, excludedProvider)
{
    protected override string Caption => "Allowed content types";

    protected override string SourceBindingColumn => nameof(FolderScopeContentTypeAllowedInfo.FolderScopeContentTypeAllowedFolderScopeID);

    protected override string TargetBindingColumn => nameof(FolderScopeContentTypeAllowedInfo.FolderScopeContentTypeAllowedClassID);
}
