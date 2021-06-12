using EventSourcing.Projections;

namespace EventSourcing.Test.Data
{
    public class TestProjection : EventProjection<TestView>
    {
        public TestProjection(TestView view) : base(view)
        {
        }
    }
}
