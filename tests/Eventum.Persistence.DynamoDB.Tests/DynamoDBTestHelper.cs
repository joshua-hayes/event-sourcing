using Eventum.Persistence.DynamoDB.Tests.TestData;
using Eventum.Reflection.TypeResolution;
using Eventum.Serialisation.Json;
using Eventum.Telemetry;
using Moq;

namespace Eventum.Persistence.DynamoDB.Tests;

public class DynamoDBTestHelper : TestHelper<DynamoDBEventStore>
{
    public DynamoDBTestHelper()
    {
        AddParam("typeResolver", new UnknownTypeResolver());
        AddParam("serialiser", new JsonEventSerialiser(new Mock<ITelemetryProvider>().Object));
        AddParam("tableName", "events");
    }

    public BankAccountEventStream SetupEventStream(Guid accountId,
                                                   string accountName = "joe-bloggs",
                                                   double balance = 100.00)
    {
        var streamId = $"BankAccount:{accountId}";
        return new BankAccountEventStream(streamId, accountId, accountName, balance);
    }
}
