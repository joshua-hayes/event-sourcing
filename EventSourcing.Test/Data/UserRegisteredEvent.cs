using EventSourcing.Events;

namespace EventSourcing.Test.Data
{
    public class UserRegisteredEvent : EventStreamEvent
    {
        public UserRegisteredEvent(int age)
        {
            Age = age;
        }

        public int Age { get; set; }
    }
}
