namespace Eventum.Persistence.InMemory.Tests;
using Eventum.EventSourcing;

public partial class InMemoryStoreTests
{
    public class LoadTestStartedEvent : EventStreamEvent
    {
        public LoadTestStartedEvent(string streamId, string id)
        {
            StreamId = streamId;
            Id = id;
        }
    }

    public class LoadTestEvent : EventStreamEvent
    {
        public LoadTestEvent(string streamId, string id)
        {
            StreamId = streamId;
            Id = id;
        }
    }

    public class LoadTestEventStream : EventStream,
                                       IEventHandler<LoadTestStartedEvent>,
                                       IEventHandler<LoadTestEvent>
    {
        public LoadTestEventStream()
        {
        }

        public LoadTestEventStream(string streamId, string id)
        {
            ApplyChange(new LoadTestStartedEvent(streamId, id));
        }

        public void Handle(LoadTestStartedEvent @event)
        {
            StreamId = @event.StreamId;
            Version = @event.Version;
        }

        public void Handle(LoadTestEvent @event)
        {
            Version = @event.Version;
        }
    }
}
