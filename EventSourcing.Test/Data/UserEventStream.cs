using EventSourcing.Events;

namespace EventSourcing.Test.Data
{
    /// <summary>
    ///  This is a test event for the purposes of testing.
    /// </summary>
    public class UserEventStream : EventStream, IEventHandler<UserRegisteredEvent>
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public UserEventStream()
        {
            ApplyChange(new UserRegisteredEvent("Elon Musk", 50));
        }

        public void Handle(UserRegisteredEvent @event)
        {
            Name = @event.Name;
            Age = @event.Age;
        }
    }
}
