using Kentico.Xperience.Admin.Base.Forms;
using Xunit;

namespace XperienceCommunity.WorkspaceRestrictions.Tests;

public class ContentTypePickerTests
{
    [Fact]
    public void ClearsSelectionWhenItsTypeIsNoLongerAllowed()
    {
        var selected = new TileSelectorItem { Identifier = 1 };
        var retained = new TileSelectorItem { Identifier = 2 };
        var properties = new TileSelectorClientProperties { Items = [selected, retained], Value = selected };

        WorkspaceContentTypeFilterExtender.FilterContentTypes(properties,
            new ContentTypeRestriction(new HashSet<int>(), new HashSet<int>()),
            [new ContentTypeRestriction(new HashSet<int> { 2 }, new HashSet<int>())]);

        Assert.Same(retained, Assert.Single(properties.Items));
        Assert.Null(properties.Value);
    }

    [Fact]
    public void PreservesExistingSelectionAndDoesNotAddTypesMissingFromKenticosPicker()
    {
        var selected = new TileSelectorItem { Identifier = 2 };
        var properties = new TileSelectorClientProperties { Items = [selected], Value = selected };

        WorkspaceContentTypeFilterExtender.FilterContentTypes(properties,
            new ContentTypeRestriction(new HashSet<int> { 1, 2 }, new HashSet<int>()),
            [new ContentTypeRestriction(new HashSet<int> { 1, 2, 3 }, new HashSet<int>())]);

        Assert.Same(selected, Assert.Single(properties.Items));
        Assert.Same(selected, properties.Value);
    }

    [Fact]
    public void StaleFolderAllowListProducesAnEmptyPickerAndClearsSelection()
    {
        var selected = new TileSelectorItem { Identifier = 1 };
        var properties = new TileSelectorClientProperties { Items = [selected], Value = selected };

        WorkspaceContentTypeFilterExtender.FilterContentTypes(properties,
            new ContentTypeRestriction(new HashSet<int> { 1 }, new HashSet<int>()),
            [new ContentTypeRestriction(new HashSet<int> { 2 }, new HashSet<int>())]);

        Assert.Empty(properties.Items);
        Assert.Null(properties.Value);
    }

    [Fact]
    public void InheritedExclusionClearsSelectionEvenWhenChildScopeAllowsIt()
    {
        var selected = new TileSelectorItem { Identifier = 1 };
        var retained = new TileSelectorItem { Identifier = 2 };
        var properties = new TileSelectorClientProperties { Items = [selected, retained], Value = selected };

        WorkspaceContentTypeFilterExtender.FilterContentTypes(properties,
            new ContentTypeRestriction(new HashSet<int>(), new HashSet<int>()),
            [
                new ContentTypeRestriction(new HashSet<int>(), new HashSet<int> { 1 }),
                new ContentTypeRestriction(new HashSet<int> { 1, 2 }, new HashSet<int>()),
            ]);

        Assert.Same(retained, Assert.Single(properties.Items));
        Assert.Null(properties.Value);
    }
}
