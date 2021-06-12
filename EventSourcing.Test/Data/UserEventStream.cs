using EventSourcing.Events;

namespace EventSourcing.Test.Data
{
    public class UserEventStream : EventStream, IEventHandler<UserRegisteredEvent>
    {
        public int Age { get; set; }

        public UserEventStream()
        {
            ApplyChange(new UserRegisteredEvent(20));
        }

        public void Handle(UserRegisteredEvent @event)
        {
            Age = @event.Age;
        }
    }
}
