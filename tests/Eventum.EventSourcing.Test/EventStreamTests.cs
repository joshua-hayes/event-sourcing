using Eventum.EventSourcing.Test.Data;
using System;
using System.Collections.Generic;
using Xunit;

namespace Eventum.EventSourcing.Test
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
        public void Expect_LoadFromSnapshot_Restores_Superclass_Properties()
        {
            // Arrange

            var expectedEventStream = new UserEventStream();
            expectedEventStream.StreamId = Guid.NewGuid().ToString();
            expectedEventStream.Version = 99;
            var snapshot = expectedEventStream.SaveToSnapshot();

            // Act

            var newEventStream = new UserEventStream();
            newEventStream.LoadFromSnapshot(snapshot);

            // Assert

            Assert.Equal(expectedEventStream.StreamId, newEventStream.StreamId);
            Assert.Equal(expectedEventStream.Version, newEventStream.Version);
        }

        [Fact]
        public void Expect_LoadFromSnapshot_Restores_Subclass_Properties()
        {
            // Arrange

            var expectedEventStream = new UserEventStream();
            expectedEventStream.Name = "Joe Blogs";
            expectedEventStream.Age = 6;
            var snapshot = expectedEventStream.SaveToSnapshot();


            // Act

            var newEventStream = new UserEventStream();
            newEventStream.LoadFromSnapshot(snapshot);

            // Assert

            Assert.Equal(expectedEventStream.StreamId, newEventStream.StreamId);
            Assert.Equal(expectedEventStream.Version, newEventStream.Version);
            Assert.Equal(expectedEventStream.Name, newEventStream.Name);
            Assert.Equal(expectedEventStream.Age, newEventStream.Age);
        }
    }
}
