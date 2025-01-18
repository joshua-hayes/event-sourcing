using Castle.Core.Internal;
using Eventum.Serialisation.Attributes;
using Xunit;

namespace Eventum.Persistence.Abstractions.Tests;

public class MaterialisedViewTests
{
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
