using System.Runtime.CompilerServices;
using Eventum.EventSourcing;

[assembly: InternalsVisibleTo("Eventum.EventSourcing.Test")]
namespace Eventum.Persistence.DynamoDB.Tests.TestData;

public class BankAccountEventStream : EventStream, IEventHandler<AccountOpenedEvent>
{
    private string _accountHolderName;
    private double _balance;
    private DateTime _modified;

    // Internal properties for testing
    internal string AccountHolderName => _accountHolderName;
    internal double Balance => _balance;
    internal DateTime Modified => _modified;

    public BankAccountEventStream(string streamId, Guid accountId, string accountHolderName, double balance)
    {
        ApplyChange(new AccountOpenedEvent(streamId, accountId, accountHolderName, balance));
    }

    public void Handle(AccountOpenedEvent @event)
    {
        StreamId = @event.StreamId;
        _accountHolderName = @event.Data.AccountHolderName;
        _balance = @event.Data.Balance;
        _modified = @event.EventTime;
    }
}