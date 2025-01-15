using EventSourcing.Events;
using EventSourcing.Projection;
using EventSourcing.Test.Data;

namespace EventSourcing.Projection.Tests.Data
{
    public class TestProjection : EventProjection<TestView>,
                                  IEventHandler<UserRegisteredEvent>
    {
        public TestProjection(TestView view) : base(view)
        {
        }

        public static new string GetViewName(IEventStreamEvent @event)
        {
            return typeof(TestView).Name + ":1";
        }

        public void Handle(UserRegisteredEvent @event)
        {
            View.Name = @event.Name;
            View.Registered = @event.EventTime;
        }
    }
}
