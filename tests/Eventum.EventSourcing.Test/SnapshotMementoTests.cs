using Eventum.EventSourcing.Test.Data;
using System.Collections.Generic;
using System;
using Xunit;

namespace Eventum.EventSourcing.Test
{
    public class SnapshotMementoTests
    {
        [Fact]
        public void When_StateDoesntExist_Expect_GetStateReturnsNull()
        {
            // Arrange

            var obj = new TestEventStream();

            // Act

            var snapshot = new SnapshotMemento(obj);
            var state = snapshot.GetState("doesnt-exist");

            // Assert

            Assert.Null(state);
        }

        [Theory]
        [InlineData(nameof(UserEventStream.StreamId))]
        [InlineData(nameof(UserEventStream.Version))]
        [InlineData(nameof(UserEventStream.Name))]
        [InlineData(nameof(UserEventStream.Age))]
        public void Expect_GetState_Returns_Expected_UserEventStream_State(string propertyName)
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

            var snapshot = new SnapshotMemento(eventStream);
            var snapshotPropertyName = snapshot.GetState(propertyName);
            var expectedPropertyName = eventStream.GetType().GetProperty(propertyName).GetValue(eventStream);

            // Assert

            Assert.Equal(expectedPropertyName, snapshotPropertyName);
        }
    }
}
