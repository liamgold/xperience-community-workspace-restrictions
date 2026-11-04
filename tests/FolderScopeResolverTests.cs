using System.Data;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using Xunit;

namespace XperienceCommunity.WorkspaceRestrictions.Tests;

public class FolderScopeResolverTests
{
    static FolderScopeResolverTests()
    {
        // Supply the schema in memory so the real Info objects do not look it up in a CMS database.
        ObjectFactory<IDataClass>.SetObjectTypeTo(typeof(IDataClass).Assembly.GetType("CMS.DataEngine.SimpleDataClass", throwOnError: true)!);
        ContentFolderInfo.TYPEINFO.ClassStructureInfo = new InMemoryClassStructure(ContentFolderInfo.TYPEINFO,
            (nameof(ContentFolderInfo.ContentFolderID), typeof(int)),
            (nameof(ContentFolderInfo.ContentFolderParentFolderID), typeof(int)),
            (nameof(ContentFolderInfo.ContentFolderWorkspaceID), typeof(int)));
        WorkspaceFolderScopeInfo.TYPEINFO.ClassStructureInfo = new InMemoryClassStructure(WorkspaceFolderScopeInfo.TYPEINFO,
            (nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeID), typeof(int)),
            (nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeFolderID), typeof(int)),
            (nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeWorkspaceID), typeof(int)),
            (nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeApplyToChildFolders), typeof(bool)));
    }

    [Theory]
    [InlineData(1, false, true)]
    [InlineData(1, true, true)]
    [InlineData(2, false, false)]
    [InlineData(2, true, true)]
    [InlineData(3, false, false)]
    [InlineData(3, true, true)]
    [InlineData(4, true, false)]
    public void ScopeAppliesToItsFolderAndOnlyOptedInDescendants(int folderId, bool applyToChildFolders, bool expected)
    {
        var folders = new[] { Folder(1), Folder(2, 1), Folder(3, 2), Folder(4) }.ToDictionary(folder => folder.ContentFolderID);
        var scope = Scope(10, 1, applyToChildFolders);

        var ids = FolderScopeResolver.GetApplicableScopeIds(folders[folderId], [scope],
            id => folders.GetValueOrDefault(id));

        Assert.Equal(expected ? [10] : Array.Empty<int>(), ids);
    }

    [Fact]
    public void UnsetInheritanceFlagDoesNotAffectDescendants()
    {
        var scope = Scope(10, 1);
        scope.SetValue(nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeApplyToChildFolders), DBNull.Value);

        var ids = FolderScopeResolver.GetApplicableScopeIds(Folder(2, 1), [scope],
            id => (ContentFolderInfo?)Folder(1));

        Assert.Empty(ids);
    }

    [Fact]
    public void ChildScopeDoesNotStopInheritanceFromGrandparent()
    {
        var folders = new[] { Folder(1), Folder(2, 1), Folder(3, 2) }.ToDictionary(folder => folder.ContentFolderID);
        var scopes = new[] { Scope(10, 1, true), Scope(20, 2), Scope(30, 3) };

        var ids = FolderScopeResolver.GetApplicableScopeIds(folders[3], scopes,
            id => folders.GetValueOrDefault(id));

        Assert.Equal([10, 30], ids);
    }

    [Fact]
    public void IncludesEveryApplicableAncestorScope()
    {
        var folders = new[] { Folder(1), Folder(2, 1), Folder(3, 2) }.ToDictionary(folder => folder.ContentFolderID);
        var scopes = new[] { Scope(10, 1, true), Scope(20, 2, true), Scope(30, 3) };

        var ids = FolderScopeResolver.GetApplicableScopeIds(folders[3], scopes,
            id => folders.GetValueOrDefault(id));

        Assert.Equal([10, 20, 30], ids);
    }

    [Fact]
    public void DisablingInheritanceKeepsScopeOnItsFolder()
    {
        var parent = Folder(1);
        var child = Folder(2, 1);
        var scope = Scope(10, 1, true);
        ContentFolderInfo? GetFolder(int id) => parent;

        var before = FolderScopeResolver.GetApplicableScopeIds(child, [scope], GetFolder);
        Assert.Equal([10], before);
        scope.WorkspaceFolderScopeApplyToChildFolders = false;
        Assert.Empty(FolderScopeResolver.GetApplicableScopeIds(child, [scope], GetFolder));
        var direct = FolderScopeResolver.GetApplicableScopeIds(parent, [scope], GetFolder);
        Assert.Equal([10], direct);
    }

    [Fact]
    public void ReassigningScopeMovesInheritanceToNewFolderTree()
    {
        var folders = new[] { Folder(1), Folder(2, 1), Folder(3), Folder(4, 3) }.ToDictionary(folder => folder.ContentFolderID);
        var scope = Scope(10, 1, true);
        ContentFolderInfo? GetFolder(int id) => folders.GetValueOrDefault(id);

        var before = FolderScopeResolver.GetApplicableScopeIds(folders[2], [scope], GetFolder);
        Assert.Equal([10], before);
        Assert.Empty(FolderScopeResolver.GetApplicableScopeIds(folders[4], [scope], GetFolder));

        scope.WorkspaceFolderScopeFolderID = 3;

        Assert.Empty(FolderScopeResolver.GetApplicableScopeIds(folders[2], [scope], GetFolder));
        var after = FolderScopeResolver.GetApplicableScopeIds(folders[4], [scope], GetFolder);
        Assert.Equal([10], after);
    }

    [Fact]
    public void MovingFolderChangesItsInheritedScopes()
    {
        var folders = new[] { Folder(1), Folder(2, 1), Folder(3) }.ToDictionary(folder => folder.ContentFolderID);
        var scopes = new[] { Scope(10, 1, true), Scope(30, 3, true) };
        ContentFolderInfo? GetFolder(int id) => folders.GetValueOrDefault(id);

        var before = FolderScopeResolver.GetApplicableScopeIds(folders[2], scopes, GetFolder);
        Assert.Equal([10], before);
        folders[2].ContentFolderParentFolderID = 3;
        var after = FolderScopeResolver.GetApplicableScopeIds(folders[2], scopes, GetFolder);
        Assert.Equal([30], after);
    }

    [Fact]
    public void ScopesAndAncestorsFromOtherWorkspacesAreIgnored()
    {
        var child = Folder(2, 1);
        var parent = Folder(1, workspaceId: 2);
        var scopes = new[] { Scope(10, 1, true), Scope(20, 2, workspaceId: 2), Scope(30, 2) };

        var ids = FolderScopeResolver.GetApplicableScopeIds(child, scopes,
            id => (ContentFolderInfo?)parent);

        Assert.Equal([30], ids);
    }

    [Fact]
    public void MissingParentDoesNotDiscardDirectScope()
    {
        var scopes = new[] { Scope(10, 1, true), Scope(20, 2) };

        var ids = FolderScopeResolver.GetApplicableScopeIds(Folder(2, 1), scopes,
            id => (ContentFolderInfo?)null);

        Assert.Equal([20], ids);
    }

    [Fact]
    public void CyclicHierarchyTerminatesWithoutDuplicatingScopes()
    {
        var folders = new[] { Folder(1, 2), Folder(2, 1) }.ToDictionary(folder => folder.ContentFolderID);
        var scopes = new[] { Scope(10, 1, true), Scope(20, 2) };

        var ids = FolderScopeResolver.GetApplicableScopeIds(folders[2], scopes,
            id => folders.GetValueOrDefault(id));

        Assert.Equal([10, 20], ids);
    }

    private static ContentFolderInfo Folder(int id, int parentId = 0, int workspaceId = 1) => new(Row(
        (nameof(ContentFolderInfo.ContentFolderID), id),
        (nameof(ContentFolderInfo.ContentFolderParentFolderID), parentId),
        (nameof(ContentFolderInfo.ContentFolderWorkspaceID), workspaceId)));

    private static WorkspaceFolderScopeInfo Scope(int id, int folderId, bool applyToChildFolders = false, int workspaceId = 1) => new(Row(
        (nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeID), id),
        (nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeFolderID), folderId),
        (nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeWorkspaceID), workspaceId),
        (nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeApplyToChildFolders), applyToChildFolders)));

    private static DataRow Row(params (string Name, object Value)[] fields)
    {
        var table = new DataTable();
        foreach (var (name, value) in fields)
        {
            table.Columns.Add(name, value.GetType());
        }

        return table.Rows.Add(fields.Select(field => field.Value).ToArray());
    }

    private sealed class InMemoryClassStructure : ClassStructureInfo
    {
        public InMemoryClassStructure(ObjectTypeInfo typeInfo, params (string Name, Type Type)[] fields)
        {
            InitCollections();
            ClassName = typeInfo.ObjectClassName;
            IDColumn = typeInfo.IDColumn;
            foreach (var (name, type) in fields)
            {
                RegisterColumn(name, type);
            }
        }
    }
}
