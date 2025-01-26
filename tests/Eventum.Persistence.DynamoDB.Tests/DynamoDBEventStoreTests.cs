using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Eventum.Persistence.DynamoDB.Tests.TestData;
using Eventum.Reflection.TypeResolution;
using Eventum.Serialisation;
using Eventum.Serialisation.Json;
using Eventum.Telemetry;
using Moq;
using System.IO;
using System.Xml.Linq;
using Xunit;

namespace Eventum.Persistence.DynamoDB.Tests;

public class DynamoDBEventStoreTests
{
    [Fact]
    public async Task Expect_SaveStreamAsync_SuccessfulSave_ReturnsTrue()
    {
        // Arrange
        var testHelper = new DynamoDBTestHelper();
        testHelper.Mock<ITelemetryProvider>();
        testHelper.Mock<IAmazonDynamoDB>()
                  .Setup(client => client.QueryAsync(It.IsAny<QueryRequest>(), default))
                  .ReturnsAsync(new QueryResponse
                  {
                      Items = new List<Dictionary<string, AttributeValue>>()
                  })
                  .Verifiable(Times.Once);

        testHelper.Mock<IAmazonDynamoDB>()
                  .Setup(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default))
                  .ReturnsAsync(new PutItemResponse
                  {
                      ConsumedCapacity = new ConsumedCapacity
                      {
                          TableName = "events",
                          CapacityUnits = 1
                      }
                  })
                  .Verifiable(Times.Once);

        var eventStream = testHelper.SetupEventStream(Guid.NewGuid());

        // Act
        var result = await testHelper.Build()
                                     .SaveStreamAsync(eventStream, 0);

        // Assert
        Assert.True(result);
        testHelper.VerifyAll();
    }

    [Fact]
    public async Task SaveStreamAsync_SuccessfulSave_ReturnsTrue()
    {
        // Arrange
        var testHelper = new DynamoDBTestHelper();
        testHelper.Mock<ITelemetryProvider>();
        testHelper.Mock<IAmazonDynamoDB>()
                  .Setup(client => client.QueryAsync(It.IsAny<QueryRequest>(), default))
                  .ReturnsAsync(new QueryResponse
                  {
                      Items = new List<Dictionary<string, AttributeValue>>()
                  })
                  .Verifiable(Times.Once);

        testHelper.Mock<IAmazonDynamoDB>()
                  .Setup(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default))
                  .ReturnsAsync(new PutItemResponse
                  {
                      ConsumedCapacity = new ConsumedCapacity
                      {
                          TableName = "events",
                          CapacityUnits = 1
                      }
                  })
                  .Verifiable(Times.Once); 

        var accountId = Guid.NewGuid();
        var eventStream = testHelper.SetupEventStream(accountId);

        // Act

        var result = await testHelper.Build()
                                     .SaveStreamAsync(eventStream, 0);

        // Assert
        Assert.True(result);
        testHelper.VerifyAll();
        testHelper.Mock<IAmazonDynamoDB>().Verify(client =>
            client.PutItemAsync(It.Is<PutItemRequest>(request =>
                request.TableName == "events" &&
                request.Item["streamId"].S == eventStream.StreamId &&
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
        var testHelper = new DynamoDBTestHelper();
        testHelper.Mock<ITelemetryProvider>();
        testHelper.Mock<IAmazonDynamoDB>()
                  .Setup(client => client.QueryAsync(It.IsAny<QueryRequest>(), default))
                  .ReturnsAsync(new QueryResponse
                  {
                      Items = new List<Dictionary<string, AttributeValue>>()
                  })
                  .Verifiable(Times.Once);

        var eventStream = testHelper.SetupEventStream(Guid.NewGuid());

        var expectedVersion = 1;
        var latestVersion = 0;

        // Act
        
        var result = await testHelper.Build()
                                     .SaveStreamAsync(eventStream, expectedVersion);

        // Assert
        Assert.False(result);
        testHelper.VerifyAll();
        testHelper.Mock<ITelemetryProvider>()
                  .Verify(telemetry => telemetry.TrackEvent(
                        "DynamoDBEventStore.SaveStreamAsync.VersionMismatch",
                        It.Is<Dictionary<string, string>>(d =>
                            d["StreamId"] == eventStream.StreamId &&
                            d["ExpectedVersion"] == expectedVersion.ToString() &&
                            d["ActualVersion"] == latestVersion.ToString()),
                        TelemetryVerbosity.Warning), Times.Once);
    }

    [Fact]
    public async Task WhenErrorOccurs_Expect_SaveStreamAsync_TracksException()
    {
        // Arrange
        var testHelper = new DynamoDBTestHelper();
        var testException = new Exception("Test exception");

        testHelper.Mock<ITelemetryProvider>();
        testHelper.Mock<IAmazonDynamoDB>()
                  .Setup(client => client.QueryAsync(It.IsAny<QueryRequest>(), default))
                  .ThrowsAsync(testException);

        var expectedVersion = 1;
        var eventStream = testHelper.SetupEventStream(Guid.NewGuid());

        // Act

        var ex = await Assert.ThrowsAsync<Exception>(() => testHelper.Build()
                                                                     .SaveStreamAsync(eventStream, expectedVersion));

        Assert.Equal("Test exception", ex.Message);
        testHelper.VerifyAll();
        testHelper.Mock<ITelemetryProvider>()
                  .Verify(telemetry => telemetry.TrackException(testException,
                        It.Is<Dictionary<string, string>>(d =>
                            d["Operation"] == "SaveStreamAsync" &&
                            d["StreamId"] == eventStream.StreamId &&
                            d["ErrorMessage"] == "Test exception"
                        ), TelemetryVerbosity.Error), Times.Once);
    }

    [Fact]
    public async Task Expect_SaveStreamAsync_TracksMetricWithNoException()
    {
        // Arrange

        var testHelper = new DynamoDBTestHelper();
        var eventStream = testHelper.SetupEventStream(Guid.NewGuid());
        var expectedVersion = 0;
        var expectedConsumedCapacity = 2.0;

        testHelper.Mock<ITelemetryProvider>();
        testHelper.Mock<IAmazonDynamoDB>()
                  .Setup(client => client.QueryAsync(It.IsAny<QueryRequest>(), default))
                  .ReturnsAsync(new QueryResponse
                  {
                     Items = new List<Dictionary<string, AttributeValue>>(),
                  });

        testHelper.Mock<IAmazonDynamoDB>()
                  .Setup(client => client.PutItemAsync(It.IsAny<PutItemRequest>(), default))
                  .ReturnsAsync(new PutItemResponse()
                  {
                      ConsumedCapacity = new ConsumedCapacity
                      {
                          TableName = "events",
                          CapacityUnits = expectedConsumedCapacity
                      }
                  });

        // Act

        var result = await testHelper.Build()
                                     .SaveStreamAsync(eventStream, expectedVersion);

        // Assert

        Assert.True(result);
        testHelper.VerifyAll();
        testHelper.Mock<ITelemetryProvider>()
                  .Verify(tp => tp.TrackMetric("DynamoDBEventStore.SaveStreamAsync.Time",
                    It.IsAny<double>(),
                    null,
                    TelemetryVerbosity.Info),
                    Times.Once);
        testHelper.Mock<ITelemetryProvider>()
                  .Verify(telemetry => telemetry.TrackMetric("DynamoDBEventStore.SaveStreamAsync.ConsumedCapacity",
                        expectedConsumedCapacity,
                        null,
                        TelemetryVerbosity.Info), Times.Once);
        testHelper.Mock<ITelemetryProvider>()
                  .Verify(tp => tp.TrackException(It.IsAny<Exception>(),
                                                It.IsAny<IDictionary<string, string>>(),
                                                It.IsAny<TelemetryVerbosity>()),Times.Never
                  );
    }

    [Fact]
    public async Task Expect_LoadStreamAsync_TracksMetricWithNoException()
    {
        // Arrange
        var testHelper = new DynamoDBTestHelper();
        var eventStream = testHelper.SetupEventStream(Guid.NewGuid());

        var accountOpened = new AccountOpenedEvent(eventStream.StreamId, Guid.Parse(eventStream.AccountId), eventStream.AccountHolderName, eventStream.Balance)
        {
            Version = 1,
            EventType = nameof(AccountOpenedEvent)
        };
        var serializedEventData = new JsonEventSerialiser(new Mock<ITelemetryProvider>().Object).Serialise(accountOpened);
        var expectedConsumedCapacity = 5.0;

        testHelper.Mock<ITelemetryProvider>();
        testHelper.Mock<IAmazonDynamoDB>()
                  .Setup(client => client.QueryAsync(It.IsAny<QueryRequest>(), default))
                           .ReturnsAsync(new QueryResponse
                           {
                               Items = new List<Dictionary<string, AttributeValue>>
                                {
                                    new Dictionary<string, AttributeValue>
                                    {
                                        { "streamId", new AttributeValue { S = eventStream.StreamId } },
                                        { "version", new AttributeValue { N = "1" } },
                                        { "id", new AttributeValue { S = accountOpened.Id } },
                                        { "eventType", new AttributeValue { S = nameof(AccountOpenedEvent) } },
                                        { "eventTime", new AttributeValue { N = new DateTimeOffset(accountOpened.EventTime).ToUnixTimeSeconds().ToString() } },
                                        { "data", new AttributeValue { S = serializedEventData } }
                                    }
                                },
                               ConsumedCapacity = new ConsumedCapacity
                               {
                                   TableName = "events",
                                   CapacityUnits = expectedConsumedCapacity
                               }
                           });


        // Act

        var result = await testHelper.Build()
                                     .LoadStreamAsync<BankAccountEventStream>(eventStream.StreamId);

        // Assert

        Assert.NotNull(eventStream);
        Assert.Single(eventStream.Events);
        Assert.IsType<AccountOpenedEvent>(eventStream.Events.First());

        testHelper.VerifyAll();
        testHelper.Mock<ITelemetryProvider>().Verify(telemetry => telemetry.TrackMetric("DynamoDBEventStore.LoadStreamAsync.Time",
                                                                         It.IsAny<double>(),
                                                                         null,
                                                                         TelemetryVerbosity.Info), Times.Once);
        testHelper.Mock<ITelemetryProvider>().Verify(telemetry => telemetry.TrackMetric("DynamoDBEventStore.LoadStreamAsync.ConsumedCapacity",
                                                                         expectedConsumedCapacity,
                                                                         null,
                                                                         TelemetryVerbosity.Info), Times.Once);
    }

    [Fact]
    public async Task WhenErrorOccurs_Expect_LoadStreamAsync_TracksException()
    {
        // Arrange

        var testHelper = new DynamoDBTestHelper();
        var eventStream = testHelper.SetupEventStream(Guid.NewGuid());
        var testException = new Exception("Test exception");

        testHelper.Mock<ITelemetryProvider>();
        testHelper.Mock<IAmazonDynamoDB>().Setup(client => client.QueryAsync(It.IsAny<QueryRequest>(), default))
                           .ThrowsAsync(testException);
        // Act & Assert

        var ex = await Assert.ThrowsAsync<Exception>(() => testHelper.Build().LoadStreamAsync<BankAccountEventStream>(eventStream.StreamId));
        Assert.Equal("Test exception", ex.Message);

        testHelper.VerifyAll();
        testHelper.Mock<ITelemetryProvider>()
                  .Verify(telemetry => telemetry.TrackException(testException, It.Is<Dictionary<string, string>>(d =>
                                                                d["Operation"] == "LoadStreamAsync" &&
                                                                d["StreamId"] == eventStream.StreamId &&
                                                                d["ErrorMessage"] == testException.Message
                                                            ),
                                            TelemetryVerbosity.Error),
                    Times.Once);
    }
}