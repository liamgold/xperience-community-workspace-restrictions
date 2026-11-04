using CMS.ContentEngine;
using CMS.DataEngine;

namespace XperienceCommunity.WorkspaceRestrictions;

/// <summary>
/// Works out which content types a workspace, or a content folder in it, allows. The create page filter and the
/// move guard both use it, so they can't disagree. It's synchronous because the move guard runs inside Kentico's
/// database transaction, and an awaited query could run outside it.
/// </summary>
internal sealed class ContentTypeRestrictionResolver(
    IInfoProvider<WorkspaceContentTypeAllowedInfo> allowedProvider,
    IInfoProvider<WorkspaceContentTypeExcludedInfo> excludedProvider,
    IInfoProvider<WorkspaceFolderScopeInfo> scopeProvider,
    IInfoProvider<FolderScopeContentTypeAllowedInfo> folderAllowedProvider,
    IInfoProvider<FolderScopeContentTypeExcludedInfo> folderExcludedProvider,
    IInfoProvider<ContentFolderInfo> folderProvider)
{
    public ContentTypeRestriction GetWorkspaceRestriction(int workspaceId) =>
        ContentTypeRestriction.ForWorkspace(workspaceId, allowedProvider, excludedProvider);

    public IReadOnlyCollection<ContentTypeRestriction> GetFolderRestrictions(int workspaceId, int folderId)
    {
        if (folderId <= 0)
        {
            return [];
        }

        var folder = folderProvider.Get(folderId);
        if (folder == null || folder.ContentFolderWorkspaceID != workspaceId)
        {
            return [];
        }

        var scopes = scopeProvider.Get()
            .WhereEquals(nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeWorkspaceID), workspaceId)
            .GetEnumerableTypedResult();
        var scopeIds = FolderScopeResolver.GetApplicableScopeIds(folder, scopes, folderProvider.Get);

        if (scopeIds.Length == 0)
        {
            return [];
        }

        var allowedBindings = folderAllowedProvider.Get()
            .WhereIn(nameof(FolderScopeContentTypeAllowedInfo.FolderScopeContentTypeAllowedFolderScopeID), scopeIds)
            .GetEnumerableTypedResult()
            .ToLookup(binding => binding.FolderScopeContentTypeAllowedFolderScopeID, binding => binding.FolderScopeContentTypeAllowedClassID);
        var excludedBindings = folderExcludedProvider.Get()
            .WhereIn(nameof(FolderScopeContentTypeExcludedInfo.FolderScopeContentTypeExcludedFolderScopeID), scopeIds)
            .GetEnumerableTypedResult()
            .ToLookup(binding => binding.FolderScopeContentTypeExcludedFolderScopeID, binding => binding.FolderScopeContentTypeExcludedClassID);

        // Keep each scope separate so its allow-list precedence cannot override another scope's restrictions.
        return scopeIds.Select(id => new ContentTypeRestriction(allowedBindings[id].ToHashSet(), excludedBindings[id].ToHashSet())).ToArray();
    }

    public bool Allows(int classId, int workspaceId, int folderId) =>
        GetWorkspaceRestriction(workspaceId).Allows(classId, GetFolderRestrictions(workspaceId, folderId));
}
