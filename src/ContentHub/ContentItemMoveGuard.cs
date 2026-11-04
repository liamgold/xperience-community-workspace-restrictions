using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using Microsoft.AspNetCore.Http;

namespace XperienceCommunity.WorkspaceRestrictions;

/// <summary>
/// Stops content items being moved into a workspace or content folder that doesn't allow their content type.
/// </summary>
/// <remarks>
/// The Content hub moves one item with <see cref="ContentItemInfo"/>.Update(), and several items with a bulk update
/// inside a transaction. Throwing from either event stops the move, and Kentico shows the editor its standard
/// "move failed" error. The guard only runs during HTTP requests, so CI/CD restore, the migration tool and other
/// processes without a request aren't affected.
/// </remarks>
internal sealed class ContentItemMoveGuard(
    ContentTypeRestrictionResolver resolver,
    IInfoProvider<ContentItemInfo> contentItemProvider,
    IHttpContextAccessor? httpContextAccessor)
{
    private const string WORKSPACE_COLUMN = nameof(ContentItemInfo.ContentItemWorkspaceID);
    private const string FOLDER_COLUMN = nameof(ContentItemInfo.ContentItemContentFolderID);

    public void Register()
    {
        ContentItemInfo.TYPEINFO.Events.Update.Before += OnUpdateBefore;
        ContentItemInfo.TYPEINFO.Events.BulkUpdate.Before += OnBulkUpdateBefore;
    }

    private void OnUpdateBefore(object? sender, ObjectEventArgs e)
    {
        if (!IsHttpRequest() || e.Object is not ContentItemInfo item)
        {
            return;
        }

        if (item.ItemChanged(WORKSPACE_COLUMN) || item.ItemChanged(FOLDER_COLUMN))
        {
            EnsureAllowed([new MovedItem(item.ContentItemContentTypeID, item.ContentItemWorkspaceID, item.ContentItemContentFolderID)]);
        }
    }

    private void OnBulkUpdateBefore(object? sender, BulkUpdateEventArgs e)
    {
        // A bulk move sets both columns. Deleting a folder only resets the folder column, and isn't blocked.
        // A bulk update without column names saves each object, so OnUpdateBefore checks those items.
        if (!IsHttpRequest() || e.ChangedColumns?.Contains(WORKSPACE_COLUMN, StringComparer.OrdinalIgnoreCase) != true)
        {
            return;
        }

        // The event arguments don't include the new values, so compare the items before and after the update.
        // The check runs before Kentico commits the transaction, so throwing rolls the whole move back.
        var before = GetItems(e.WhereCondition).ToDictionary(item => item.ContentItemID, item => (item.ContentItemWorkspaceID, item.ContentItemContentFolderID));
        e.CallWhenFinished(() => EnsureAllowed(GetItems(e.WhereCondition)
            .Where(item => !before.TryGetValue(item.ContentItemID, out var original)
                || original != (item.ContentItemWorkspaceID, item.ContentItemContentFolderID))
            .Select(item => new MovedItem(item.ContentItemContentTypeID, item.ContentItemWorkspaceID, item.ContentItemContentFolderID))));
    }

    private IEnumerable<ContentItemInfo> GetItems(IWhereCondition where) => contentItemProvider.Get()
        .Where(where)
        .Columns(nameof(ContentItemInfo.ContentItemID), nameof(ContentItemInfo.ContentItemContentTypeID), WORKSPACE_COLUMN, FOLDER_COLUMN)
        .GetEnumerableTypedResult();

    private void EnsureAllowed(IEnumerable<MovedItem> items)
    {
        var blockedClassIds = items
            .Distinct()
            .Where(item => !resolver.Allows(item.ClassId, item.WorkspaceId, item.FolderId))
            .Select(item => item.ClassId)
            .Distinct()
            .ToArray();

        if (blockedClassIds.Length == 0)
        {
            return;
        }

        var names = blockedClassIds.Select(id => DataClassInfoProvider.GetDataClassInfo(id)?.ClassDisplayName ?? id.ToString());
        throw new InvalidOperationException(
            $"Workspace restrictions don't allow these content types in the target workspace or folder: {string.Join(", ", names)}.");
    }

    private bool IsHttpRequest() => httpContextAccessor?.HttpContext != null;

    private readonly record struct MovedItem(int ClassId, int WorkspaceId, int FolderId);
}
