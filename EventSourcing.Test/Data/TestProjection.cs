using EventSourcing.Events;
using EventSourcing.Projections;

namespace EventSourcing.Test.Data
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
