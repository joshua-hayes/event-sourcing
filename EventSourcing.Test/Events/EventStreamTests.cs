using EventSourcing.Events;
using EventSourcing.Test.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;

namespace EventSourcing.Test.Events
{
    public class EventStreamTests
    {
        [Fact]
        public void Expect_New_EventStream_Has_Empty_Changeset()
        {
            // Arrange / Act

            var stream = new TestEventStream();

            // Assert

            Assert.Empty(stream.UncommittedChanges);
        }

        [Fact]
        public void When_LoadFromHistory_Loads_An_Event_Without_A_Handler_Expect_EventStreamException_Is_Thrown()
        {
            // Arrange

            var history = new List<IEventStreamEvent> {
                new UserRegisteredEvent(Guid.NewGuid().ToString(), "Jack Mallers", 27)
                {
                    EventType = nameof(UserRegisteredEvent),
                }
            };
            var stream = new TestEventStream();

            // Act / Assert

            Assert.Throws<EventStreamHandlerException>(() => stream.LoadFromHistory(history));
        }

        [Fact]
        public void When_LoadFromHistory_Loads_Already_Committed_Events_Event_Expect_Version_Is_Set()
        {
            // Arrange

            int expectedVersion = 2;
            var history = new List<IEventStreamEvent> {
                new UserRegisteredEvent(Guid.NewGuid().ToString(), "Elon Musk", 50) {
                    Version = 1,
                },
                new UserRegisteredEvent(Guid.NewGuid().ToString(), "Alex Mashinsky", 55) {
                    Version = 2,
                }
            };
            var stream = new UserEventStream();

            // Act

            stream.LoadFromHistory(history);

            // Assert

            Assert.Equal(expectedVersion, stream.Version);
        }

        [Fact]
        public void When_Event_Is_LoadedFromHistory_Expect_Handler_Is_Called()
        {
            // Arrange

            int expectedHandlerValidation = 50;
            var history = new List<IEventStreamEvent> {
                new UserRegisteredEvent(Guid.NewGuid().ToString(), "Elon Musk", expectedHandlerValidation)
            };
            var stream = new UserEventStream();

            // Act

            stream.LoadFromHistory(history);

            // Assert

            Assert.Equal(expectedHandlerValidation, stream.Age);
        }

        [Fact]
        public void When_A_New_Event_Is_Applied_Expect_It_Is_Added_To_Uncommited_Events()
        {
            // Arrange / Act

            var stream = new UserEventStream(Guid.NewGuid().ToString(), "Elon Musk", 50);

            // Assert

            Assert.NotEmpty(stream.UncommittedChanges);
        }

        [Fact]
        public void Expect_SaveToSnapshot_Saves_StreamId_Version_Name_And_Age_As_JObject()
        {
            // Arrange

            var eventStream = new UserEventStream();
            var history = new List<IEventStreamEvent> {
                new UserRegisteredEvent(Guid.NewGuid().ToString(), "Elon Musk", 50) {
                    Version = 1
                }
            };
            eventStream.LoadFromHistory(history);

            // Act

            var snapshot = eventStream.SaveToSnapshot();

            // Assert
            
            Assert.Equal(eventStream.StreamId, snapshot.State.GetValue("StreamId")?.Value<string>());
            Assert.Equal(eventStream.Version, snapshot.State.GetValue("Version")?.Value<int>());
            Assert.Equal(eventStream.Name, snapshot.State.GetValue("Name")?.Value<string>());
            Assert.Equal(eventStream.Age, snapshot.State.GetValue("Age")?.Value<int>());
        }

        [Fact]
        public void Expect_LoadFromSnapshot_Restores_StreamId_Version_Name_And_Age()
        {
            // Arrange

            var eventStream = new UserEventStream();
            var streamId = Guid.NewGuid().ToString();
            var version = 2;
            var name = "Elon Musk";
            var age = 50;
            var stateStr = $"{{'streamId': '{streamId}', 'version': {version}, 'name': '{name}', 'age': {age}}}";
            var state = JObject.Parse(stateStr);
            var memento = new SnapshotMemento(state);

            // Act

            eventStream.LoadFromSnapshot(memento);

            // Assert

            Assert.Equal(streamId, eventStream.StreamId);
            Assert.Equal(version, eventStream.Version);
            Assert.Equal(name, eventStream.Name);
            Assert.Equal(age, eventStream.Age);
        }
    }
}
