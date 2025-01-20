namespace Eventum.EventSourcing.Test.Data;
/// <summary>
///  This is a test event for the purposes of testing.
/// </summary>
public class UserEventStream : EventStream,
                               ISnapshotable,
                               IEventHandler<UserRegisteredEvent>
{
    public string Name { get; set; }
    public int Age { get; set; }

    public UserEventStream()
    {
    }

    public UserEventStream(string streamId, string name, int age)
    {
        ApplyChange(new UserRegisteredEvent(streamId, name, age));
    }

    public void Handle(UserRegisteredEvent @event)
    {
        StreamId = @event.StreamId;
        Name = @event.Name;
        Age = @event.Age;
    }

    public override bool IsSnapshotable => true;
}
