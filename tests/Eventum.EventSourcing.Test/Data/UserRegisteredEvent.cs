namespace Eventum.EventSourcing.Test.Data
{
    public class UserRegisteredEvent : EventStreamEvent
    {
        public UserRegisteredEvent(string streamId, string name, int age)
        {
            StreamId = streamId;
            Age = age;
            Name = name;
        }

        public int Age { get; set; }

        public string Name { get; set; }
    }
}
