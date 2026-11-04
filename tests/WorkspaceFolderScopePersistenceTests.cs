using System.Reflection;
using Microsoft.Data.SqlClient;
using Xunit;

namespace XperienceCommunity.WorkspaceRestrictions.Tests;

public class WorkspaceFolderScopePersistenceTests
{
    [Fact]
    public void RegistersGuidAsScopeIdentity()
    {
        Assert.Equal(nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeGUID), WorkspaceFolderScopeInfo.TYPEINFO.GUIDColumn);
        Assert.Equal("[Unknown]", WorkspaceFolderScopeInfo.TYPEINFO.BinaryColumn);
    }

    [Theory]
    [InlineData(2601, true, true)]
    [InlineData(2627, true, true)]
    [InlineData(2601, false, false)]
    [InlineData(2627, false, false)]
    [InlineData(1205, true, false)]
    [InlineData(547, true, false)]
    public void OnlyHandlesDuplicateKeysForTheWorkspaceFolderIndex(int errorNumber, bool matchingIndex, bool expected)
    {
        var index = matchingIndex ? WorkspaceFolderScopePersistence.UniqueFolderIndexName : "OtherUniqueIndex";
        var exception = SqlExceptionWith((errorNumber, $"Database error for index '{index}'."));

        Assert.Equal(expected, WorkspaceFolderScopePersistence.IsDuplicateFolder(exception));
    }

    [Fact]
    public void FindsDuplicateKeyInsideKenticoExceptionWrappers()
    {
        var sqlException = SqlExceptionWith((2601, WorkspaceFolderScopePersistence.UniqueFolderIndexName));
        var exception = new InvalidOperationException("Save failed", new Exception("Database query failed", sqlException));

        Assert.True(WorkspaceFolderScopePersistence.IsDuplicateFolder(exception));
    }

    [Fact]
    public void ChecksAllSqlErrors()
    {
        var exception = SqlExceptionWith((3621, "Statement terminated"), (2601, WorkspaceFolderScopePersistence.UniqueFolderIndexName));

        Assert.True(WorkspaceFolderScopePersistence.IsDuplicateFolder(exception));
    }

    [Fact]
    public void DoesNotTreatAnUnrelatedExceptionAsADuplicate()
    {
        var exception = new InvalidOperationException(WorkspaceFolderScopePersistence.UniqueFolderIndexName);

        Assert.False(WorkspaceFolderScopePersistence.IsDuplicateFolder(exception));
    }

    private static SqlException SqlExceptionWith(params (int Number, string Message)[] errors)
    {
        // SqlClient exposes no public constructors for server errors.
        var collection = (SqlErrorCollection)Activator.CreateInstance(typeof(SqlErrorCollection), nonPublic: true)!;
        var constructor = typeof(SqlError).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
            [typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int), typeof(Exception)], null)!;
        var add = typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (var (number, message) in errors)
        {
            var error = constructor.Invoke([number, (byte)1, (byte)14, "server", message, "", 1, null]);
            add.Invoke(collection, [error]);
        }

        var create = typeof(SqlException).GetMethod("CreateException", BindingFlags.Static | BindingFlags.NonPublic,
            null, [typeof(SqlErrorCollection), typeof(string)], null)!;
        return (SqlException)create.Invoke(null, [collection, "17.0.0"])!;
    }
}
