using EventSourcing.Events;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourcing.Infrastructure.Cosmos
{
    public class CosmosEventStore : IEventStore
    {
        private readonly IEventTypeResolver _eventTypeResolver;
        private readonly CosmosClient _client;
        private readonly string _databaseId;
        private readonly string _containerId;

        public CosmosEventStore(IEventTypeResolver eventTypeResolver,
                                string endpointUrl,
                                string authorizationKey,
                                string databaseId,
                                string containerId = "events")
        {

            // TODO Refactor to inject
            _client = new CosmosClient(endpointUrl, authorizationKey);

            _eventTypeResolver = eventTypeResolver;
            _databaseId = databaseId;
            _containerId = containerId;
        }

        /// <summary>
        /// <see cref="IEventStore.LoadStreamAsync{T}(string)"/>
        /// </summary>
        public async Task<T> LoadStreamAsync<T>(string streamId) where T : EventStream
        {
            var container = _client.GetContainer(_databaseId, _containerId);

            var sqlQueryText = "SELECT * FROM e"
                            + " WHERE e.streamId = @streamId"
                            + " ORDER BY e.version";

            var queryDefinition = new QueryDefinition(sqlQueryText).WithParameter("@streamId", streamId);


            var eventStoreEvents = new List<IEventStreamEvent>();
            var feedIterator = container.GetItemQueryIterator<JObject>(queryDefinition);
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
            var container = _client.GetContainer(_databaseId, _containerId);
            var partitionKey = new PartitionKey(stream.StreamId);

            var parameters = new dynamic[]
            {
                stream.StreamId,
                expectedVersion,
                SerializeEvents(expectedVersion, stream.UncommittedChanges)
            };

            return await container.Scripts.ExecuteStoredProcedureAsync<bool>("spAppendToStream", partitionKey, parameters);
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