using Xunit;

namespace Eventum.Reflection.TypeResolution.Tests;
public class KnownTypeResolverTests
{
    private readonly KnownTypeResolver _resolver;
    private readonly string _namespace = "Eventum.Reflection.TypeResolution.Tests";
    private readonly string _assembly = "Eventum.Reflection.Tests";

    public KnownTypeResolverTests()
    {
        _resolver = new KnownTypeResolver(_assembly, _namespace);
    }

    [Fact]
    public void Expect_Resolve_ValidType_ReturnsType()
    {
        // Arrange

        var typeName = "KnownTypeResolverTests";

        // Act

        var resolvedType = _resolver.Resolve(typeName);

        // Assert

        Assert.NotNull(resolvedType);
        Assert.Equal(typeName, resolvedType.Name);
    }

    [Fact]
    public void Expect_Resolve_InvalidType_ThrowsTypeLoadException()
    {
        // Arrange

        var invalidTypeName = "InvalidTypeName";

        // Act & Assert

        var exception = Assert.Throws<TypeLoadException>(() => _resolver.Resolve(invalidTypeName));
        Assert.Equal($"Type '{_namespace}.{invalidTypeName}, {_assembly}' could not be resolved.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Expect_Resolve_NullOrEmptyTypeName_ThrowsArgumentNullException(string typeName)
    {
        // Act & Assert

        var exception = Assert.Throws<ArgumentNullException>(() => _resolver.Resolve(typeName));
        Assert.Equal("Value cannot be null. (Parameter 'typeName')", exception.Message);
    }
}
