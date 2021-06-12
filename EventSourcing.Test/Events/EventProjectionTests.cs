using EventSourcing.Test.Data;
using Xunit;

namespace EventSourcing.Test
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
    }
}
