using Castle.Core.Internal;
using Eventum.Serialisation.Attributes;
using Xunit;

namespace Eventum.Persistence.Abstractions.Tests;

public class MaterialisedViewTests
{
    [Fact]
    public void WhenMaterialisedViewInitialised_Expect_Properties_SetByConstructor()
    {
        // Arrange

        string expectedView = "view";
        string expectedeTag = "etag";
        var expectedChange = "change";
        var changes = new List<string> { expectedChange };

        // Act

        var view = new MaterialisedView(expectedView, expectedeTag, changes);

        // Assert

        Assert.Equal(expectedView, view.View);
        Assert.Equal(expectedeTag, view.Etag);
        Assert.Contains(expectedChange, changes);
    }

    [Theory]
    [InlineData(nameof(MaterialisedView.Etag))]
    [InlineData(nameof(MaterialisedView.Changeset))]
    [InlineData(nameof(MaterialisedView.View))]
    public void Expect_Properties_Decorated_With_IgnoreSerialisation(string propertyToIgnore)
    {
        // Assert

        var ignored = typeof(MaterialisedView).GetType().GetAttributes<IgnoreSerializationAttribute>() != null;
        Assert.True(ignored);
    }
}
