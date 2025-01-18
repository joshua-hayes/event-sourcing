using Eventum.EventSourcing;
using System;
using Xunit;

namespace Eventum.EventSourcing.Test
{
    public class EventStreamEventTests
    {
        [Fact]
        public void Expect_New_EventStreamEvent_Sets_EventTime()
        {
            // Arrange / Act

            var @event = new EventStreamEvent();

            // Assert

            Assert.NotEqual(DateTime.MinValue, @event.EventTime);
        }
    }
}
