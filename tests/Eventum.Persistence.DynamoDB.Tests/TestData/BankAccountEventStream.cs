using System.Runtime.CompilerServices;
using Amazon.Runtime;
using Eventum.EventSourcing;

[assembly: InternalsVisibleTo("Eventum.Persistence.DynamoDB.Tests;")]
namespace Eventum.Persistence.DynamoDB.Tests.TestData;

public class BankAccountEventStream : EventStream, IEventHandler<AccountOpenedEvent>
{
    private string _accountHolderName;
    private double _balance;
    private DateTime _modified;
    private string _accountId;

    // Internal properties for testing

    internal string AccountId => _accountId;
    internal string AccountHolderName => _accountHolderName;
    internal double Balance => _balance;
    internal DateTime Modified => _modified;

    internal IList<IEventStreamEvent> Events => _events;

    public BankAccountEventStream()
    {
    }

    public BankAccountEventStream(string streamId, Guid accountId, string accountHolderName, double balance)
    {
        ApplyChange(new AccountOpenedEvent(streamId, accountId, accountHolderName, balance));
    }

    public void Handle(AccountOpenedEvent @event)
    {
        StreamId = @event.StreamId;
        _accountId = @event.Id;
        _accountHolderName = @event.Data.AccountHolderName;
        _balance = @event.Data.Balance;
        _modified = @event.EventTime;
    }
}