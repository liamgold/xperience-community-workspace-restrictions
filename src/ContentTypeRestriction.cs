using CMS.DataEngine;

namespace XperienceCommunity.WorkspaceRestrictions;

internal sealed class ContentTypeRestriction(IReadOnlySet<int> allowedClassIds, IReadOnlySet<int> excludedClassIds)
{
    public bool Allows(int classId, IEnumerable<ContentTypeRestriction>? folderRestrictions = null) =>
        (allowedClassIds.Count > 0 ? allowedClassIds.Contains(classId) : !excludedClassIds.Contains(classId))
        && (folderRestrictions?.All(restriction => restriction.Allows(classId)) ?? true);

    public ObjectQuery ApplyTo(ObjectQuery query) => allowedClassIds.Count > 0
        ? query.WhereIn(nameof(DataClassInfo.ClassID), allowedClassIds)
        : query.WhereNotIn(nameof(DataClassInfo.ClassID), excludedClassIds);

    // Synchronous so that the move guard can call it inside Kentico's database transaction.
    public static ContentTypeRestriction ForWorkspace(
        int workspaceId,
        IInfoProvider<WorkspaceContentTypeAllowedInfo> allowedProvider,
        IInfoProvider<WorkspaceContentTypeExcludedInfo> excludedProvider)
    {
        var allowedClassIds = allowedProvider.Get()
            .WhereEquals(nameof(WorkspaceContentTypeAllowedInfo.WorkspaceContentTypeAllowedWorkspaceID), workspaceId)
            .GetEnumerableTypedResult()
            .Select(b => b.WorkspaceContentTypeAllowedClassID)
            .ToHashSet();

        var excludedClassIds = excludedProvider.Get()
            .WhereEquals(nameof(WorkspaceContentTypeExcludedInfo.WorkspaceContentTypeExcludedWorkspaceID), workspaceId)
            .GetEnumerableTypedResult()
            .Select(b => b.WorkspaceContentTypeExcludedClassID)
            .ToHashSet();

        return new ContentTypeRestriction(allowedClassIds, excludedClassIds);
    }
}
