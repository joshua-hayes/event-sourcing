using Eventum.Events;

namespace Eventum.Projection.Tests.Data
{
    public class TestProjection : EventProjection<TestView>,
                                  IEventHandler<TestEvent1>,
                                  IEventHandler<TestEvent2>
    {
        public TestProjection(TestView view, int maxChangesetSize = 10) : base(view, maxChangesetSize)
        {
        }

        public static new string GetViewName(IEventStreamEvent @event)
        {
            return typeof(TestView).Name + ":1";
        }

        public void Handle(TestEvent1 @event)
        {
            View.Field1 = @event.Field1;
            View.Field2 = @event.Field2;
        }

        public void Handle(TestEvent2 @event)
        {
            View.Field3 = @event.Field3;
            View.Field4 = @event.Field4;
        }
    }
}
