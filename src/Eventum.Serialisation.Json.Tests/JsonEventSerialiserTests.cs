using System.Text.Json;
using Xunit;

namespace Eventum.Serialisation.Json.Tests;

public class JsonEventSerialiserTests
{
    private readonly JsonEventSerialiser _jsonEventSerialiser;

    public JsonEventSerialiserTests()
    {
        _jsonEventSerialiser = new JsonEventSerialiser();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WhenDataIsNullOrEmpty_Expect_Deserialise_ThrowsArgumentNullException(string data)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _jsonEventSerialiser.Deserialise<TestObject>(data));
    }

    [Fact]
    public void WhenDataIsValid_Expect_Deserialise_ReturnsCorrectObject()
    {
        // Arrange

        string data = "{\"Property1\": \"Value1\", \"Property2\": 123}";
        var expected = new TestObject { Property1 = "Value1", Property2 = 123 };

        // Act

        var actual = _jsonEventSerialiser.Deserialise<TestObject>(data);

        // Assert

        Assert.Equal(expected.Property1, actual.Property1);
        Assert.Equal(expected.Property2, actual.Property2);
    }

    [Fact]
    public void WhenObjectIsNull_Expect_Serialise_ThrowsArgumentNullException()
    {
        // Arrange

        TestObject obj = null;

        // Act & Assert

        Assert.Throws<ArgumentNullException>(() => _jsonEventSerialiser.Serialise(obj));
    }

    [Fact]
    public void WhenObjectIsValid_Expect_Serialise_ReturnsCorrectJson()
    {
        // Arrange

        var obj = new TestObject { Property1 = "Value1", Property2 = 123 };
        var expectedJson = "{\"Property1\":\"Value1\",\"Property2\":123}";
        var jsonEventSerialiser = new JsonEventSerialiser(new JsonSerializerOptions { WriteIndented = false });

        // Act

        var actualJson = jsonEventSerialiser.Serialise(obj);

        // Assert

        Assert.Equal(expectedJson, actualJson);
    }

    private class TestObject
    {
        public string Property1 { get; set; }
        public int Property2 { get; set; }
    }
}
