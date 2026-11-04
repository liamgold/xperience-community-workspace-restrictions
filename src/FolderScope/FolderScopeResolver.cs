using CMS.ContentEngine;

namespace XperienceCommunity.WorkspaceRestrictions;

internal static class FolderScopeResolver
{
    public static int[] GetApplicableScopeIds(ContentFolderInfo folder,
        IEnumerable<WorkspaceFolderScopeInfo> scopes, Func<int, ContentFolderInfo?> getFolder)
    {
        var workspaceScopes = scopes.Where(scope => scope.WorkspaceFolderScopeWorkspaceID == folder.ContentFolderWorkspaceID).ToArray();
        var folderIds = new HashSet<int> { folder.ContentFolderID };

        if (workspaceScopes.Any(scope => scope.WorkspaceFolderScopeApplyToChildFolders))
        {
            var parentId = folder.ContentFolderParentFolderID;
            while (parentId > 0 && !folderIds.Contains(parentId))
            {
                var parent = getFolder(parentId);
                if (parent == null || parent.ContentFolderWorkspaceID != folder.ContentFolderWorkspaceID)
                {
                    break;
                }

                folderIds.Add(parent.ContentFolderID);
                parentId = parent.ContentFolderParentFolderID;
            }
        }

        return workspaceScopes.Where(scope => scope.WorkspaceFolderScopeFolderID == folder.ContentFolderID
                || (scope.WorkspaceFolderScopeApplyToChildFolders && folderIds.Contains(scope.WorkspaceFolderScopeFolderID)))
            .Select(scope => scope.WorkspaceFolderScopeID).ToArray();
    }
}
