
using Eventum.EventSourcing;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Eventum.Persistence.DynamoDB.Tests;")]
namespace Eventum.Persistence.DynamoDB.Tests.TestData;

public class AccountOpenedEvent : IEventStreamEvent<AccountOpenedEventData>
{
    public AccountOpenedEvent()
    {
    }
    public AccountOpenedEvent(string streamId, Guid accountId, string accountHolderName, double balance)
    {
        StreamId = streamId;
        Id = accountId.ToString();
        EventTime = DateTime.UtcNow;
        Data = new AccountOpenedEventData
        {
            Balance = balance,
            AccountHolderName = accountHolderName
        };
    }

    public string StreamId { get; set; }
    public string Id { get; set; }
    public string EventType { get; set; }
    public DateTime EventTime { get; set; }
    public int Version { get; set; }
    public AccountOpenedEventData Data { get; set; }
}

public class AccountOpenedEventData
{
    public string AccountHolderName { get; set; }
    public double Balance { get; set; }
}
