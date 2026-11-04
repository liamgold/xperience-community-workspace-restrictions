using System.Data;
using System.Data.Common;
using CMS.DataEngine;
using Microsoft.Data.SqlClient;
using Xunit;

namespace XperienceCommunity.WorkspaceRestrictions.Tests;

public class FolderScopeDatabaseTests
{
    [SqlServerFact]
    public async Task ConcurrentCreatesCannotSaveTwoScopesForTheSameFolder()
    {
        await using var database = await ScopeTable.Create();
        await database.InstallIndex();
        await database.InstallIndex(); // Application restarts must leave the index intact.

        var results = await Race(database, 10,
            "INSERT INTO {0} (WorkspaceFolderScopeWorkspaceID, WorkspaceFolderScopeFolderID) VALUES (1, 10)",
            "INSERT INTO {0} (WorkspaceFolderScopeWorkspaceID, WorkspaceFolderScopeFolderID) VALUES (1, 10)");

        Assert.Single(results, error => error == null);
        Assert.True(WorkspaceFolderScopePersistence.IsDuplicateFolder(Assert.Single(results.OfType<SqlException>())));
        Assert.Equal(1, await database.Count("WorkspaceFolderScopeWorkspaceID = 1 AND WorkspaceFolderScopeFolderID = 10"));

        // Uniqueness covers the pair, not either column individually.
        await database.Execute("INSERT INTO {0} (WorkspaceFolderScopeWorkspaceID, WorkspaceFolderScopeFolderID) VALUES (2, 10), (1, 11)");
        Assert.Equal(3, await database.Count("1 = 1"));
    }

    [SqlServerFact]
    public async Task ConcurrentReassignmentsLeaveTheLosingScopeUnchanged()
    {
        await using var database = await ScopeTable.Create();
        await database.InstallIndex();
        await database.Execute("INSERT INTO {0} (WorkspaceFolderScopeWorkspaceID, WorkspaceFolderScopeFolderID) VALUES (1, 10), (1, 20)");

        var results = await Race(database, 30,
            "UPDATE {0} SET WorkspaceFolderScopeFolderID = 30 WHERE WorkspaceFolderScopeFolderID = 10",
            "UPDATE {0} SET WorkspaceFolderScopeFolderID = 30 WHERE WorkspaceFolderScopeFolderID = 20");

        Assert.Single(results, error => error == null);
        var conflict = Assert.Single(results.OfType<SqlException>());
        Assert.True(WorkspaceFolderScopePersistence.IsDuplicateFolder(new InvalidOperationException("Save failed", conflict)));
        Assert.Equal(1, await database.Count("WorkspaceFolderScopeFolderID = 30"));
        Assert.Equal(1, await database.Count("WorkspaceFolderScopeFolderID IN (10, 20)"));
    }

    [SqlServerFact]
    public async Task InstallingIndexDoesNotSilentlyRemoveExistingDuplicates()
    {
        await using var database = await ScopeTable.Create();
        await database.Execute("INSERT INTO {0} (WorkspaceFolderScopeWorkspaceID, WorkspaceFolderScopeFolderID) VALUES (1, 10), (1, 10)");

        await Assert.ThrowsAsync<SqlException>(() => database.InstallIndex());

        Assert.Equal(2, await database.Count("1 = 1"));
    }

    private static async Task<Exception?[]> Race(ScopeTable database, int targetFolderId, params string[] statements)
    {
        // Synchronize after both requests have passed the UI's existence check.
        using var ready = new Barrier(statements.Length);
        return await Task.WhenAll(statements.Select(statement => Task.Run(async () =>
        {
            Assert.Equal(0, await database.Count($"WorkspaceFolderScopeWorkspaceID = 1 AND WorkspaceFolderScopeFolderID = {targetFolderId}"));
            Assert.True(ready.SignalAndWait(TimeSpan.FromSeconds(20)));
            return await Record.ExceptionAsync(() => database.Execute(statement));
        })));
    }

    private sealed class ScopeTable(string connectionString) : IAsyncDisposable
    {
        private readonly string tableName = "WorkspaceScopeTests_" + Guid.NewGuid().ToString("N");

        public static async Task<ScopeTable> Create()
        {
            var database = new ScopeTable(Environment.GetEnvironmentVariable(SqlServerFactAttribute.ConnectionStringVariable)!);
            await database.Execute("CREATE TABLE {0} (WorkspaceFolderScopeID int IDENTITY PRIMARY KEY, WorkspaceFolderScopeWorkspaceID int NOT NULL, WorkspaceFolderScopeFolderID int NOT NULL)");
            return database;
        }

        public async Task InstallIndex()
        {
            using var captured = new IndexConnection();
            WorkspaceRestrictionsInstaller.InstallFolderScopeIndex(new IndexTableManager(captured));

            // Execute the actual Kentico-generated DDL against an isolated test table.
            await using var sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();
            var sourceTable = WorkspaceFolderScopeInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
            await using var command = new SqlCommand(captured.Sql.Replace($"[{sourceTable}]", $"[{tableName}]", StringComparison.Ordinal), sqlConnection);
            foreach (var parameter in captured.Parameters)
            {
                command.Parameters.AddWithValue(parameter.Name, parameter.Name == "@TableName" ? tableName : parameter.Value);
            }
            await command.ExecuteNonQueryAsync();
        }

        public async Task Execute(string sql)
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await using var command = new SqlCommand(string.Format(sql, $"[dbo].[{tableName}]"), connection);
            await command.ExecuteNonQueryAsync();
        }

        public async Task<int> Count(string predicate)
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await using var command = new SqlCommand($"SELECT COUNT(*) FROM [dbo].[{tableName}] WHERE {predicate}", connection);
            return (int)(await command.ExecuteScalarAsync())!;
        }

        public async ValueTask DisposeAsync() => await Execute("DROP TABLE {0}");
    }

    private sealed class IndexTableManager(IDataConnection connection) : AbstractTableManager
    {
        protected override IDataConnection GetConnection() => connection;
    }

    private sealed class IndexConnection() : AbstractDataConnection(null)
    {
        public string Sql { get; private set; } = "";
        public QueryDataParameters Parameters { get; private set; } = new();

        public override int ExecuteNonQuery(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool requiresTransaction)
        {
            Sql = queryText;
            Parameters = queryParams;
            return 0;
        }

        public override void BulkInsert(DataTable sourceData, string targetTable, BulkInsertSettings? insertSettings = null) => throw new NotSupportedException();
        protected override IDbConnection CreateNativeConnection() => throw new NotSupportedException();
        protected override DbCommand CreateCommand(string cmdText) => throw new NotSupportedException();
        protected override DbParameter CreateParameter(DataParameter parameter) => throw new NotSupportedException();
        protected override DbDataAdapter CreateDataAdapter() => throw new NotSupportedException();
    }
}

public sealed class SqlServerFactAttribute : FactAttribute
{
    internal const string ConnectionStringVariable = "WORKSPACE_RESTRICTIONS_TEST_CONNECTION_STRING";

    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionStringVariable)))
        {
            Skip = $"Set {ConnectionStringVariable} to run SQL Server integration tests using temporary test tables.";
        }
    }
}
