using Xunit;

namespace XperienceCommunity.WorkspaceRestrictions.Tests;

public class ContentTypeRestrictionTests
{
    public static TheoryData<int[], int[], int[]?, int[]?, int[]> Scenarios => new()
    {
        // Workspace behavior, including its existing allow-list precedence.
        { [], [], null, null, [1, 2, 3, 4] },
        { [1, 2], [], null, null, [1, 2] },
        { [], [2], null, null, [1, 3, 4] },
        { [1, 2], [2, 3], null, null, [1, 2] },
        // Empty scopes inherit the workspace; populated scopes narrow it.
        { [1, 2], [], [], [], [1, 2] },
        { [], [], [2, 3], [], [2, 3] },
        { [], [], [], [2, 3], [1, 4] },
        { [1, 2, 3], [], [2, 3, 4], [], [2, 3] },
        { [1, 2, 3], [], [], [2, 4], [1, 3] },
        { [], [1, 4], [1, 2, 4], [], [2] },
        { [], [1, 4], [], [2], [3] },
        // Allow-list precedence applies independently at each level.
        { [1, 2, 3], [2], [2, 3], [3], [2, 3] },
        // A stale folder allow list cannot expose workspace-disallowed types or fall back to all types.
        { [1, 2], [], [3, 4], [1], [] },
        { [], [3, 4], [3, 4], [], [] },
        // Excluding everything is valid.
        { [1, 2], [], [], [1, 2], [] },
    };

    [Theory]
    [MemberData(nameof(Scenarios))]
    public void FiltersContentTypesWithinWorkspaceAndFolderLimits(
        int[] workspaceAllowed, int[] workspaceExcluded, int[]? folderAllowed, int[]? folderExcluded, int[] expected)
    {
        var workspace = new ContentTypeRestriction(workspaceAllowed.ToHashSet(), workspaceExcluded.ToHashSet());
        var folder = folderAllowed == null ? null
            : new ContentTypeRestriction(folderAllowed.ToHashSet(), folderExcluded!.ToHashSet());

        var actual = Enumerable.Range(1, 4).Where(id => workspace.Allows(id, folder == null ? [] : [folder]));

        Assert.Equal(expected, actual);
    }

    public static TheoryData<int[], int[], int[], int[], int[], int[], int[]> InheritedScenarios => new()
    {
        // Empty child scopes retain inherited restrictions.
        { [1, 2, 3], [], [2, 3], [], [], [], [2, 3] },
        // Child allow lists cannot restore types excluded by an ancestor.
        { [], [], [], [2], [1, 2], [], [1] },
        // Each allow list narrows the set; exclusions remain local to their scope.
        { [1, 2, 3], [], [1, 2], [2], [2, 3], [2], [2] },
        { [], [], [], [1], [], [2], [3, 4] },
        // Conflicting allow lists can intentionally leave no available types.
        { [], [], [1], [], [2], [], [] },
        // Workspace restrictions remain the upper limit.
        { [], [1, 2], [1, 2, 3], [], [2, 3, 4], [], [3] },
        { [], [], [1, 2, 3], [], [], [2], [1, 3] },
    };

    [Theory]
    [MemberData(nameof(InheritedScenarios))]
    public void CombinesWorkspaceAncestorAndChildRestrictions(
        int[] workspaceAllowed, int[] workspaceExcluded, int[] parentAllowed, int[] parentExcluded,
        int[] childAllowed, int[] childExcluded, int[] expected)
    {
        var workspace = new ContentTypeRestriction(workspaceAllowed.ToHashSet(), workspaceExcluded.ToHashSet());
        ContentTypeRestriction[] scopes =
        [
            new(parentAllowed.ToHashSet(), parentExcluded.ToHashSet()),
            new(childAllowed.ToHashSet(), childExcluded.ToHashSet()),
        ];

        var actual = Enumerable.Range(1, 4).Where(id => workspace.Allows(id, scopes));

        Assert.Equal(expected, actual);
    }
}
