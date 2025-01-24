using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Eventum.EventSourcing;
using Eventum.Persistence;
using Eventum.Serialisation;
using Eventum.Telemetry;
using System.Diagnostics;
using System.IO;

/// <summary>
/// Provides DynamoDB persistence support for saving and loading events to / from
/// an <see cref="IEventStore"/> respectively.
/// </summary>
public class DynamoDBEventStore : IEventStore
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly ITelemetryProvider _telemetryProvider;
    private readonly IEventSerialiser _serialiser;
    private readonly IEventTypeResolver _eventTypeResolver;
    private readonly string _tableName;

    /// <summary>
    /// Instantiates a new <see cref="DynamoDBEventStore"/>.
    /// </summary>
    /// <param name="dynamoDbClient">The DynamoDB client.</param>
    /// <param name="telemetryProvider">The provider used to track telemetry.</param>
    /// <param name="serialiser">The serialiser used to serialise / de-serialise events.</param>
    /// <param name="eventTypeResolver">The resolver used to resolve the type of an event. </param>
    /// <param name="tableName">The event table name.</param>
    public DynamoDBEventStore(IAmazonDynamoDB dynamoDbClient,
                              ITelemetryProvider telemetryProvider,
                              IEventSerialiser serialiser,
                              IEventTypeResolver eventTypeResolver,
                              string tableName)
    {
        _dynamoDbClient = dynamoDbClient;
        _telemetryProvider = telemetryProvider;
        _serialiser = serialiser;
        _eventTypeResolver = eventTypeResolver;
        _tableName = tableName;
    }

    /// <inheritdoc />
    /// <exception cref="NotImplementedException"></exception>
    public async Task<T> LoadStreamAsync<T>(string streamId) where T : EventStream
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<bool> SaveStreamAsync(EventStream stream, int expectedVersion)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var latestVersion = await GetStreamVersionAsync(stream.StreamId);
            if (latestVersion != expectedVersion)
            {
                _telemetryProvider.TrackEvent("DynamoDBEventStore.SaveStreamAsync.VersionMismatch", new Dictionary<string, string>
                {
                    { "StreamId", stream.StreamId },
                    { "ExpectedVersion", expectedVersion.ToString() },
                    { "ActualVersion", latestVersion.ToString() }
                }, TelemetryVerbosity.Warning);

                return false;
            }

            int currentVersion = expectedVersion;
            await SaveEventsAsync(stream, expectedVersion);

            _telemetryProvider.TrackMetric("DynamoDBEventStore.SaveStreamAsync.Time", stopwatch.ElapsedMilliseconds);
            return true;
        }
        catch (Exception ex)
        {
            _telemetryProvider.TrackException(ex, new Dictionary<string, string>
            {
                { "Operation", "SaveStreamAsync" },
                { "StreamId", stream.StreamId },
                { "ErrorMessage", ex.Message },
                { "StackTrace", ex.StackTrace },
            });
            throw;
        }
    }

    /// <summary>
    /// Gets the stream version (latest event version).
    /// </summary>
    /// <param name="streamId">The identifier of the stream.</param>
    /// <returns>The stream version.</returns>
    private async Task<int> GetStreamVersionAsync(string streamId)
    {
        // Get the latest event for the stream
        var queryRequest = new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "streamId = :streamId", // :streamId is a placeholder for expression attribute value
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                    { ":streamId", new AttributeValue { S = streamId } }
                },

            ScanIndexForward = false, // Retrieve in descening order
            Limit = 1,                // Only the latest item
            ConsistentRead = true     // Strongly consistent read
        };

        var response = await _dynamoDbClient.QueryAsync(queryRequest);
        var latestEvent = response.Items.FirstOrDefault();

        return latestEvent != null ? int.Parse(latestEvent["version"].N) : 0;
    }

    /// <summary>
    /// Saves the event stream to the event store.
    /// </summary>
    /// <param name="stream">The stream to save.</param>
    /// <param name="expectedVersion">The expected version of the stream.</param>
    /// <returns>The task.</returns>
    private async Task SaveEventsAsync(EventStream stream, int expectedVersion)
    {
        foreach (var @event in stream.UncommittedChanges)
        {
            @event.EventType = @event.GetType().Name;
            @event.Version = ++expectedVersion;
            @event.EventTime = @event.EventTime == default(DateTime)
                                                        ? DateTime.UtcNow
                                                        : @event.EventTime;

            var data = _serialiser.Serialise(((dynamic)@event).Data);

            var putRequest = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "streamId", new AttributeValue { S = @event.StreamId } },
                    { "version", new AttributeValue { N = @event.Version.ToString() } },
                    { "id", new AttributeValue { S = @event.Id } },
                    { "eventType", new AttributeValue { S = @event.EventType } },
                    { "eventTime", new AttributeValue { N = new DateTimeOffset(@event.EventTime).ToUnixTimeSeconds().ToString() } },
                    { "data", new AttributeValue { S = data } }
                }
            };

            await _dynamoDbClient.PutItemAsync(putRequest);
        }
    }


}
