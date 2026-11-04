using Microsoft.Data.SqlClient;

namespace XperienceCommunity.WorkspaceRestrictions;

internal static class WorkspaceFolderScopePersistence
{
    internal const string UniqueFolderIndexName = "IX_XperienceCommunity_WorkspaceFolderScope_WorkspaceID_FolderID";
    internal const string DuplicateFolderMessage = "A scope already exists for this folder. Edit the existing scope instead.";

    public static bool IsDuplicateFolder(Exception exception)
    {
        // Kentico may wrap the SQL exception. Only handle conflicts with our workspace/folder index.
        for (Exception? current = exception; current != null; current = current.InnerException)
        {
            if (current is SqlException sqlException && sqlException.Errors.Cast<SqlError>().Any(error =>
                error.Number is 2601 or 2627
                && error.Message.Contains(UniqueFolderIndexName, StringComparison.Ordinal)))
            {
                return true;
            }
        }

        return false;
    }
}
