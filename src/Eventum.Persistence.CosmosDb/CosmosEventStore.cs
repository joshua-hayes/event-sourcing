using Eventum.EventSourcing;
using Eventum.Persistence.Abstractions;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventum.Persistence.CosmosDb
{
    public class CosmosEventStore : IEventStore
    {
        private readonly IEventTypeResolver _eventTypeResolver;
        private readonly CosmosClient _client;
        private readonly Container _container;

        public CosmosEventStore(IEventTypeResolver eventTypeResolver,
                                CosmosClient client,
                                string databaseId,
                                string containerId = "events")
        {
            _client = client;
            _container = _client.GetContainer(databaseId, containerId);
            _eventTypeResolver = eventTypeResolver;
        }

        /// <summary>
        /// <see cref="IEventStore.LoadStreamAsync{T}(string)"/>
        /// </summary>
        public async Task<T> LoadStreamAsync<T>(string streamId) where T : EventStream
        {
            var sqlQueryText = "SELECT * FROM e"
                            + " WHERE e.streamId = @streamId"
                            + " ORDER BY e.version";

            var queryDefinition = new QueryDefinition(sqlQueryText).WithParameter("@streamId", streamId);


            var eventStoreEvents = new List<IEventStreamEvent>();
            var feedIterator = _container.GetItemQueryIterator<JObject>(queryDefinition);
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync();
                foreach (var jObjects in response)
                {
                    var typeString = jObjects.GetValue("eventType").ToString();
                    var properEventType = _eventTypeResolver.Resolve(typeString);
                    var eventStoreEvent = (IEventStreamEvent)jObjects.ToObject(properEventType);

                    eventStoreEvents.Add(eventStoreEvent);
                }
            }

            var stream = Activator.CreateInstance<T>();
            stream.LoadFromHistory(eventStoreEvents);

            return stream;
        }

        /// <summary>
        /// <see cref="IEventStore.SaveStreamAsync(EventStream, int)'"/>
        /// </summary>
        public async Task<bool> SaveStreamAsync(EventStream stream, int expectedVersion)
        {
            var partitionKey = new PartitionKey(stream.StreamId);

            var parameters = new dynamic[]
            {
                stream.StreamId,
                expectedVersion,
                SerializeEvents(expectedVersion, stream.UncommittedChanges)
            };

            return await _container.Scripts.ExecuteStoredProcedureAsync<bool>("spAppendToStream", partitionKey, parameters);
        }

        private static string SerializeEvents(int expectedVersion, IEnumerable<IEventStreamEvent> events)
        {
            var items = events.Select(e =>
            {
                e.EventType = e.GetType().Name;
                e.Version = ++expectedVersion;

                return e;
            });
            return JsonConvert.SerializeObject(items);
        }
    }
}