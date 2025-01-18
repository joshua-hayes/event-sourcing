using Eventum.EventSourcing;
using Eventum.Serialisation.Json.Attributes;
using Eventum.Serialisation.Json.TestData;
using System.Text.Json;
using Xunit;

namespace Eventum.Serialisation.Json.Converters.Tests;


public class IgnoreSerializationAttributeJsonConverterTests
{
    private readonly JsonEventSerialiser _jsonEventSerialiser;

    public IgnoreSerializationAttributeJsonConverterTests()
    {
        _jsonEventSerialiser = new JsonEventSerialiser();
    }

    [Fact]
    public void WhenSerialising_Expect_SerializationAttributeProperties_AreIgnored()
    {
        // Arrange

        var obj = new TestObject { Property1 = "Value1", Property2 = "Value2", Property3 = "IgnoredValue" };
        var expectedJson = "{\"Property1\":\"Value1\",\"Property2\":\"Value2\"}";

        // Act

        var actualJson = _jsonEventSerialiser.Serialise(obj);

        // Assert

        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public void WhenDeserialising_Expect_SerializationAttributeProperties_AreIgnored()
    {
        // Arrange

        string data = "{\"Property1\": \"Value1\", \"Property2\": \"Value2\", \"Property3\": \"Value3\" }";
        var expected = new TestObject { Property1 = "Value1", Property2 = "Value2", Property3 = null };

        // Act

        var actual = _jsonEventSerialiser.Deserialise<TestObject>(data);

        // Assert

        Assert.Equal(expected.Property1, actual.Property1);
        Assert.Equal(expected.Property2, actual.Property2);
        Assert.Null(actual.Property3);
    }

    [Theory]
    [InlineData(nameof(EventStream.IsSnapshotable))]
    [InlineData(nameof(EventStream.UncommittedChanges))]
    public void Expect_SnapshotProperties_AreNot_Serialised(string propertyToIgnore)
    {
        // Arrange
        var customOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            Converters = { new IgnoreSerializationAttributeJsonConverter() },
        };
        var serialiser = new JsonEventSerialiser(customOptions);
        var eventStream = new TestEventStream();

        // Act

        var json = serialiser.Serialise(eventStream);
        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(json, customOptions);

        // Assert

        Assert.False(properties?.ContainsKey(propertyToIgnore));
    }
}