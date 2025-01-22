using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Eventum.EventSourcing;
using Eventum.Persistence;
using Eventum.Persistence.DynamoDB;
using Eventum.Persistence.DynamoDB.Tests.TestData;
using Eventum.Telemetry;
using Moq;
using Xunit;

namespace Eventum.Persistence.DynamoDB.Tests;

public class DynamoDBConnectivityTests
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly Mock<ITelemetryProvider> _mockTelemetryProvider;
    private readonly IEventStore _eventStore;

    public DynamoDBConnectivityTests()
    {
        _dynamoDbClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig
        {
            RegionEndpoint = RegionEndpoint.APSoutheast2,
        });
    }

    [Fact]
    public async Task CanPutAndGetItem()
    {
        var tableName = "events";
        var itemKey = "testItemKey";
        var itemValue = "testItemValue";

        // Put item into DynamoDB table
        var putRequest = new PutItemRequest
        {
            TableName = tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "streamId", new AttributeValue { S = itemKey } },
                { "version", new AttributeValue { N = "1" } },
                { "eventType", new AttributeValue { S = itemValue } }
            }
        };

        await _dynamoDbClient.PutItemAsync(putRequest);

        // Get item from DynamoDB table
        var getRequest = new GetItemRequest
        {
            TableName = tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "streamId", new AttributeValue { S = itemKey } },
                { "version", new AttributeValue { N = "1" } }
            }
        };

        var getResponse = await _dynamoDbClient.GetItemAsync(getRequest);

        // Validate that the item was retrieved correctly
        Assert.True(getResponse.Item["eventType"].S == itemValue);
    }
}
