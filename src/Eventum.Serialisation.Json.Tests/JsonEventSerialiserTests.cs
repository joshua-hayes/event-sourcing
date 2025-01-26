using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Eventum.Serialisation.Json.TestData;
using Moq;
using Eventum.Telemetry;
using Xunit.Sdk;

namespace Eventum.Serialisation.Json.Tests;

public partial class JsonEventSerialiserTests
{
    private readonly JsonEventSerialiser _jsonEventSerialiser;
    private Mock<ITelemetryProvider> _mockTelemetryProvider;

    public JsonEventSerialiserTests()
    {
        _mockTelemetryProvider = new Mock<ITelemetryProvider>();
        _jsonEventSerialiser = new JsonEventSerialiser(_mockTelemetryProvider.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WhenDataIsNullOrEmpty_Expect_DeserialiseByGeneric_ThrowsArgumentNullException(string data)
    {
        // Act & Assert

        Assert.Throws<ArgumentNullException>(() => _jsonEventSerialiser.Deserialise<TestObject>(data));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WhenDataIsNullOrEmpty_Expect_DeserialiseByType_ThrowsArgumentNullException(string data)
    {
        // Act

        var type = typeof(TestObject);

        // Assert

        Assert.Throws<ArgumentNullException>(() => _jsonEventSerialiser.Deserialise(data, type));
    }

    [Fact]
    public void WhenDataIsValid_Expect_DeserialiseByGeneric_ReturnsCorrectObject()
    {
        // Arrange

        string data = "{\"Property1\": \"Value1\", \"Property2\": \"Value2\"}";
        var expected = new TestObject { Property1 = "Value1", Property2 = "Value2" };

        // Act

        var actual = _jsonEventSerialiser.Deserialise<TestObject>(data);

        // Assert

        Assert.Equal(expected.Property1, actual.Property1);
        Assert.Equal(expected.Property2, actual.Property2);
    }

    [Fact]
    public void WhenDataIsValid_Expect_DeserialiseByType_ReturnsCorrectObject()
    {
        // Arrange

        string data = "{\"Property1\": \"Value1\", \"Property2\": \"Value2\"}";
        var expected = new TestObject { Property1 = "Value1", Property2 = "Value2" };
        var type = typeof(TestObject);

        // Act

        var actual = (TestObject)_jsonEventSerialiser.Deserialise(data, type);

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

        var obj = new TestObject { Property1 = "Value1", Property2 = "Value2", Property3 = "Value3" };
        var expectedJson = "{\"Property1\":\"Value1\",\"Property2\":\"Value2\"}";

        // Act

        var actualJson = _jsonEventSerialiser.Serialise(obj);

        // Assert

        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public void WhenCustomOptionsArePassed_Expect_JsonSerializerOptionsToBeSetCorrectly()
    {
        // Arrange

        var customOptions = new JsonSerializerOptions {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = false
        };
        var serialiser = new JsonEventSerialiser(customOptions, _mockTelemetryProvider.Object);

        // Act

        var options = serialiser.Options;
        
        // Assert
        
        Assert.Equal(JsonIgnoreCondition.WhenWritingNull, options.DefaultIgnoreCondition);
        Assert.False(options.WriteIndented);
    }

    [Fact]
    public void WhenJsonNamingPolicyIsCamelCase_Expect_SerialisedProperties_Are_CamelCase()
    {
        // Arrange

        var customOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var obj = new TestObject { Property1 = "Value1", Property2 = "Value2" };
        var serialiser = new JsonEventSerialiser(customOptions, _mockTelemetryProvider.Object);

        // Act

        var json = serialiser.Serialise(obj);

        // Assert

        var expectedJson = "{\"property1\":\"Value1\",\"property2\":\"Value2\"}";
        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void Expect_Serialise_TracksMetricAndNoException()
    {
        // Arrange

        var obj = new TestObject { Property1 = "Value1", Property2 = "Value2" };

        // Act

        var jsonString = _jsonEventSerialiser.Serialise(obj);

        // Assert

        _mockTelemetryProvider.Verify(tp => tp.TrackMetric("JsonEventSerialiser.Serialise.Time",
                                                           It.IsAny<double>(),
                                                           null,
                                                           TelemetryVerbosity.Info), Times.Once);
        _mockTelemetryProvider.Verify(tp => tp.TrackException(It.IsAny<Exception>(),
                                                              It.IsAny<IDictionary<string, string>>(),
                                                              It.IsAny<TelemetryVerbosity>()),
                                      Times.Never);
    }

    [Fact]
    public void Expect_DeserialiseByGeneric_TracksMetricAndNoException()
    {
        // Arrange

        var jsonString = "{\"id\":1,\"name\":\"Test\"}";

        // Act

        var obj = _jsonEventSerialiser.Deserialise<TestClass>(jsonString);

        // Assert

        _mockTelemetryProvider.Verify(tp => tp.TrackMetric("JsonEventSerialiser.Deserialise.Time",
                                                           It.IsAny<double>(),
                                                           null,
                                                           TelemetryVerbosity.Info), Times.Once);
        _mockTelemetryProvider.Verify(tp => tp.TrackException(It.IsAny<Exception>(),
                                                              It.IsAny<IDictionary<string, string>>(),
                                                              It.IsAny<TelemetryVerbosity>()), Times.Never);
    }

    [Fact]
    public void Expect_DeserialiseByType_TracksMetricAndNoException()
    {
        // Arrange

        var jsonString = "{\"id\":1,\"name\":\"Test\"}";
        var type = typeof(TestClass);

        // Act

        var obj = _jsonEventSerialiser.Deserialise(jsonString, type);

        // Assert

        _mockTelemetryProvider.Verify(tp => tp.TrackMetric("JsonEventSerialiser.Deserialise.Time",
                                                           It.IsAny<double>(),
                                                           null,
                                                           TelemetryVerbosity.Info), Times.Once);
        _mockTelemetryProvider.Verify(tp => tp.TrackException(It.IsAny<Exception>(),
                                                              It.IsAny<IDictionary<string, string>>(),
                                                              It.IsAny<TelemetryVerbosity>()), Times.Never);
    }

    [Fact]
    public void WhenErrorOccurs_Expect_Serialise_TracksException()
    {
        // Arrange

        var mockSerialiser = new Mock<IEventSerialiser>();
        mockSerialiser.Setup(s => s.Serialise(It.IsAny<object>())).Throws(new Exception("Serialisation Error"));

        var obj = new TestObject { Property1 = "Value1", Property2 = "Value2" };

        // Act & Assert

        Assert.Throws<ArgumentNullException>(() => _jsonEventSerialiser.Serialise<object>(null));

        _mockTelemetryProvider.Verify(tp => tp.TrackException(It.IsAny<Exception>(),
                                                              It.Is<IDictionary<string, string>>(d => d["Operation"] == "Serialise"),
                                                              TelemetryVerbosity.Error), Times.Once);
    }

    [Fact]
    public void WhenErrorOccurs_Expect_DeserialiseByGeneric_TracksException()
    {
        // Act & Assert

        Assert.Throws<ArgumentNullException>(() => _jsonEventSerialiser.Deserialise<object>(null));

        _mockTelemetryProvider.Verify(tp => tp.TrackException(It.IsAny<Exception>(),
                                                              It.Is<IDictionary<string, string>>(d => d["Operation"] == "Deserialise"),
                                                              TelemetryVerbosity.Error), Times.Once);
    }

    [Fact]
    public void WhenErrorOccurs_Expect_DeserialiseByType_TracksException()
    {
        // Act & Assert

        var type = typeof(TestObject);
        Assert.Throws<ArgumentNullException>(() => _jsonEventSerialiser.Deserialise(null, type));

        _mockTelemetryProvider.Verify(tp => tp.TrackException(It.IsAny<Exception>(),
                                                              It.Is<IDictionary<string, string>>(d => d["Operation"] == "Deserialise"),
                                                              TelemetryVerbosity.Error), Times.Once);
    }
}