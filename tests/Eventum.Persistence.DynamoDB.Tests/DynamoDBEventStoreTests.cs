using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Eventum.Persistence.DynamoDB.Tests.TestData;
using Eventum.Reflection.TypeResolution;
using Eventum.Serialisation;
using Eventum.Serialisation.Json;
using Eventum.Telemetry;
using Moq;
using Xunit;

namespace Eventum.Persistence.DynamoDB.Tests;

public class DynamoDBEventStoreTests
{
    private readonly Mock<ITelemetryProvider> _mockTelemetryProvider;
    private readonly Mock<ITypeResolver> _mockTypeResolver;
    private readonly Mock<IAmazonDynamoDB> _mockDynamoDbClient;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IEventStore _eventStore;
    private readonly IEventSerialiser _serialiser;
    private readonly string _tableName = "events";
    public DynamoDBEventStoreTests()
    {
        _dynamoDbClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig
        {
            RegionEndpoint = RegionEndpoint.APSoutheast2,
        });
        _mockTelemetryProvider = new Mock<ITelemetryProvider>();
        _mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
        _serialiser = new JsonEventSerialiser(_mockTelemetryProvider.Object);
        _mockTypeResolver = new Mock<ITypeResolver>();
        _eventStore = new DynamoDBEventStore(_mockDynamoDbClient.Object,
                                             _mockTelemetryProvider.Object,
                                             _serialiser,
                                             new UnknownTypeResolver(),
                                             _tableName);
    }

    [Fact]
    public async Task SaveStreamAsync_SuccessfulSave_ReturnsTrue()
    {
        // Arrange

        var accountId = Guid.NewGuid();
        var streamId = $"BankAccount:{accountId}";
        var accountName = "Joe Bloggs";
        var balance = 0.0;
        var eventStream = new BankAccountEventStream(streamId, accountId, accountName, balance);


        _mockDynamoDbClient.Setup(client => client.QueryAsync(It.IsAny<QueryRequest>(), default))
                           .ReturnsAsync(new QueryResponse
                           {
                               Items = new List<Dictionary<string, AttributeValue>>()
                           });

        _mockDynamoDbClient.Setup(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default))
                            .ReturnsAsync(new PutItemResponse()
                            {
                                ConsumedCapacity = new ConsumedCapacity
                                {
                                    TableName = _tableName,
                                    CapacityUnits = 1
                                }
                            });

        // Act

        var result = await _eventStore.SaveStreamAsync(eventStream, 0);

        // Assert
        Assert.True(result);
        _mockDynamoDbClient.Verify(client => client.PutItemAsync(It.Is<PutItemRequest>(request =>
            request.TableName == _tableName &&
            request.Item["streamId"].S == streamId &&
            request.Item["version"].N == "1" &&
            request.Item["id"].S == accountId.ToString() &&
            request.Item["eventType"].S == nameof(AccountOpenedEvent) &&
            request.Item["eventTime"].N == new DateTimeOffset(eventStream.Modified).ToUnixTimeSeconds()
                                                                                   .ToString()
        ), default), Times.Once);
    }

    [Fact]
    public async Task WhenVersionMismatch_Expect_SaveStreamAsync_TracksEvent()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var streamId = $"BankAccount:{accountId}";
        var accountName = "Joe Bloggs";
        var eventStream = new BankAccountEventStream(streamId, accountId, accountName, 0.0);
        var expectedVersion = 1;
        var latestVersion = 0;

        _mockDynamoDbClient.Setup(client => client.QueryAsync(It.IsAny<QueryRequest>(), default))
                           .ReturnsAsync(new QueryResponse
                           {
                               Items = new List<Dictionary<string, AttributeValue>>
                                {
                                    new Dictionary<string, AttributeValue>
                                    {
                                        { "version", new AttributeValue { N = latestVersion.ToString() } }
                                    }
                                }
                           });

        // Act
        var result = await _eventStore.SaveStreamAsync(eventStream, expectedVersion);

        // Assert
        Assert.False(result);
        _mockTelemetryProvider.Verify(telemetry => telemetry.TrackEvent(
            "DynamoDBEventStore.SaveStreamAsync.VersionMismatch",
            It.Is<Dictionary<string, string>>(d =>
                d["StreamId"] == streamId &&
                d["ExpectedVersion"] == expectedVersion.ToString() &&
                d["ActualVersion"] == latestVersion.ToString()),
            TelemetryVerbosity.Warning), Times.Once);
    }

    [Fact]
    public async Task WhenErrorOccurs_Expect_SaveStreamAsync_TracksException()
    {
        // Arrange

        var accountId = Guid.NewGuid();
        var streamId = $"BankAccount:{accountId}";
        var accountName = "Joe Bloggs";
        var eventStream = new BankAccountEventStream(streamId, accountId, accountName, 0.0);
        var expectedVersion = 1;
        var testException = new Exception("Test exception");

        _mockDynamoDbClient.Setup(client => client.QueryAsync(It.IsAny<QueryRequest>(), default))
                           .ThrowsAsync(testException);

        // Act & Assert

        var ex = await Assert.ThrowsAsync<Exception>(() => _eventStore.SaveStreamAsync(eventStream, expectedVersion));
        Assert.Equal("Test exception", ex.Message);

        _mockTelemetryProvider.Verify(telemetry => telemetry.TrackException(testException,
                                                    It.Is<Dictionary<string, string>>(d =>
                                                        d["Operation"] == "SaveStreamAsync" &&
                                                        d["StreamId"] == streamId &&
                                                        d["ErrorMessage"] == "Test exception"
                                                    ), TelemetryVerbosity.Error), Times.Once);
    }

    [Fact]
    public async Task Expect_SaveStreamAsync_TracksMetricWithNoException()
    {
        // Arrange

        var accountId = Guid.NewGuid();
        var streamId = $"BankAccount:{accountId}";
        var accountName = "Joe Bloggs";
        var eventStream = new BankAccountEventStream(streamId, accountId, accountName, 0.0);
        var expectedVersion = 0;
        var expectedConsumedCapacity = 2.0;

        _mockDynamoDbClient.Setup(client => client.QueryAsync(It.IsAny<QueryRequest>(), default))
                           .ReturnsAsync(new QueryResponse
                           {
                               Items = new List<Dictionary<string, AttributeValue>>(),
                           });

        _mockDynamoDbClient.Setup(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default))
                           .ReturnsAsync(new PutItemResponse()
                           {
                               ConsumedCapacity = new ConsumedCapacity
                               {
                                   TableName = _tableName,
                                   CapacityUnits = expectedConsumedCapacity
                               }
                           });

        // Act

        var result = await _eventStore.SaveStreamAsync(eventStream, expectedVersion);

        // Assert

        Assert.True(result);
        _mockTelemetryProvider.Verify(tp => tp.TrackMetric("DynamoDBEventStore.SaveStreamAsync.Time",
                                                  It.IsAny<double>(),
                                                  null,
                                                  TelemetryVerbosity.Info),
                                                  Times.Once);
        _mockTelemetryProvider.Verify(telemetry => telemetry.TrackMetric("DynamoDBEventStore.SaveStreamAsync.ConsumedCapacity",
                                                                         expectedConsumedCapacity,
                                                                         null,
                                                                         TelemetryVerbosity.Info), Times.Once);
        _mockTelemetryProvider.Verify(tp => tp.TrackException(It.IsAny<Exception>(),
                                                              It.IsAny<IDictionary<string, string>>(),
                                                              It.IsAny<TelemetryVerbosity>()),
                                   Times.Never);
    }

    [Fact]
    public async Task Expect_LoadStreamAsync_TracksMetricWithNoException()
    {
        // Arrange

        var streamId = Guid.NewGuid().ToString();
        var accountId = Guid.NewGuid();
        var accountOpened = new AccountOpenedEvent(streamId, accountId, "Joe Bloggs", 1000.00)
        {
            Version = 1,
            EventType = nameof(AccountOpenedEvent)
        };
        var serializedEventData = _serialiser.Serialise(accountOpened);
        var expectedConsumedCapacity = 5.0;

        _mockDynamoDbClient.Setup(client => client.QueryAsync(It.IsAny<QueryRequest>(), default))
                           .ReturnsAsync(new QueryResponse
                           {
                               Items = new List<Dictionary<string, AttributeValue>>
                                {
                                    new Dictionary<string, AttributeValue>
                                    {
                                        { "streamId", new AttributeValue { S = streamId } },
                                        { "version", new AttributeValue { N = "1" } },
                                        { "id", new AttributeValue { S = accountOpened.Id } },
                                        { "eventType", new AttributeValue { S = nameof(AccountOpenedEvent) } },
                                        { "eventTime", new AttributeValue { N = new DateTimeOffset(accountOpened.EventTime).ToUnixTimeSeconds().ToString() } },
                                        { "data", new AttributeValue { S = serializedEventData } }
                                    }
                                },
                               ConsumedCapacity = new ConsumedCapacity
                               {
                                   TableName = _tableName,
                                   CapacityUnits = expectedConsumedCapacity
                               }
                           });


        // Act

        var eventStream = await _eventStore.LoadStreamAsync<BankAccountEventStream>(streamId);

        // Assert

        Assert.NotNull(eventStream);
        Assert.Single(eventStream.Events);
        Assert.IsType<AccountOpenedEvent>(eventStream.Events.First());

        _mockTelemetryProvider.Verify(telemetry => telemetry.TrackMetric("DynamoDBEventStore.LoadStreamAsync.Time",
                                                                         It.IsAny<double>(),
                                                                         null,
                                                                         TelemetryVerbosity.Info), Times.Once);
        _mockTelemetryProvider.Verify(telemetry => telemetry.TrackMetric("DynamoDBEventStore.LoadStreamAsync.ConsumedCapacity",
                                                                         expectedConsumedCapacity,
                                                                         null,
                                                                         TelemetryVerbosity.Info), Times.Once);
    }

    [Fact]
    public async Task WhenErrorOccurs_Expect_LoadStreamAsync_TracksException()
    {
        // Arrange

        var streamId = Guid.NewGuid().ToString();
        var testException = new Exception("Test exception");

        _mockDynamoDbClient.Setup(client => client.QueryAsync(It.IsAny<QueryRequest>(), default))
                           .ThrowsAsync(testException);
        // Act & Assert

        var ex = await Assert.ThrowsAsync<Exception>(() => _eventStore.LoadStreamAsync<BankAccountEventStream>(streamId));
        Assert.Equal("Test exception", ex.Message);

        _mockTelemetryProvider.Verify(telemetry => telemetry.TrackException(testException, It.Is<Dictionary<string, string>>(d =>
                                                                                                d["Operation"] == "LoadStreamAsync" &&
                                                                                                d["StreamId"] == streamId &&
                                                                                                d["ErrorMessage"] == testException.Message
                                                                                            ),
                                                                            TelemetryVerbosity.Error),
                                                    Times.Once);
    }
}