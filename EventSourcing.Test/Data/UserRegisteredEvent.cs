using EventSourcing.Events;

namespace EventSourcing.Test.Data
{
    public class UserRegisteredEvent : EventStreamEvent
    {
        public UserRegisteredEvent(string name, int age)
        {
            Age = age;
            Name = name;
        }

        public int Age { get; set; }
        public string Name { get; set; }
    }
}
