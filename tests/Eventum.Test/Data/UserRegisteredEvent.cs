using Eventum.Events;
using System.Text.Json.Serialization;

namespace Eventum.Test.Data
{
    public class UserRegisteredEvent : EventStreamEvent
    {
        public UserRegisteredEvent(string streamId, string name, int age)
        {
            StreamId = streamId;
            Age = age;
            Name = name;
        }

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
