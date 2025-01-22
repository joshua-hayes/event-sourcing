namespace Eventum.Persistence.InMemory.Tests;
using Eventum.EventSourcing;

public partial class InMemoryStoreTests
{
    public class LoadTestStartedEvent : EventStreamEvent
    {
        public LoadTestStartedEvent(string streamId, string id, int version)
        {
            StreamId = streamId;
            Id = id;
            Version = version;
        }
    }

    public class LoadTestEvent : EventStreamEvent
    {
        public LoadTestEvent(string streamId, string id, int version)
        {
            StreamId = streamId;
            Id = id;
            Version = version;
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
            ApplyChange(new LoadTestStartedEvent(streamId, id, 1));
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
