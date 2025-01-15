using EventSourcing.Projection;
using EventSourcing.Test.Data;
using Xunit;

namespace EventSourcing.Test.Projections
{
    public class EventProjectionTests
    {
        [Fact]
        public void Expect_New_Projection_Sets_View()
        {
            // Arrange

            var view = new TestView();

            // Act

            var projection = new TestProjection(view);

            // Assert

            Assert.NotNull(projection.View);
            Assert.Equal(view, projection.View);
        }


        [Fact]
        public void When_ApplyChange_Cannot_Handle_Event_Expect_EventProjectionException_Is_Thrown()
        {
            // Arrange

            var @event = new UnhandledEvent();
            var projection = new TestProjection(new TestView());

            // Act / Assert

            Assert.Throws<EventProjectionException>(() => projection.ApplyChange(@event));
        }
    }
}
