using Eventum.Persistence.Abstractions;
using Eventum.Projection.Abstractions;
using Microsoft.Azure.Cosmos;
using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eventum.Persistence.CosmosDb
{
    /// <summary>
    /// Stores and saves <see cref="MaterialisedView"/>'s  in a cosomos view container.
    /// </summary>
    public class CosmosMaterialisedViewRepository : IMaterialisedViewRepository
    {
        private readonly CosmosClient _client;
        private readonly Container _container;

        public CosmosMaterialisedViewRepository(CosmosClient client,
                                                string databaseId,
                                                string containerId = "views")
        {
            _client = client;
            _container = _client.GetContainer(databaseId, containerId);
        }

        /// <summary>
        /// <see cref="IMaterialisedViewRepository.LoadViewAsync{TView}(string)"/>
        /// </summary>
        public async Task<bool> SaveViewAsync(string name, MaterialisedView view)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(view);
                var payload = JsonNode.Parse(jsonString) as JsonObject;

                payload.Remove("view");
                payload.Remove("_etag");
                payload.Remove("changeset");

                var viewData = new MaterialisedViewData
                {
                    Id = name,
                    Changeset = view.Changeset,
                    View = payload
                };

                var partitionKey = new PartitionKey(name);
                await _container.UpsertItemAsync(viewData,
                                                partitionKey,
                                                new ItemRequestOptions
                                                {
                                                    IfMatchEtag = view.Etag
                                                });
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return false;
            }
        }

        /// <summary>
        /// <see cref="IMaterialisedViewRepository.LoadViewAsync{TView}(string)"/>
        /// </summary>
        public async Task<TView> LoadViewAsync<TView>(string name) where TView : MaterialisedView, new()
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            try
            {
                var partitionKey = new PartitionKey(name);
                var response = await _container.ReadItemAsync<TView>(name, partitionKey);

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return new TView();
            }
        }

        /// <summary>
        /// <see cref="IMaterialisedViewRepository.LoadViewAsync(string, Type)"/>
        /// </summary>
        public async Task<MaterialisedView> LoadViewAsync(string name, Type type)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            try
            {
                var partitionKey = new PartitionKey(name);
                var response = await _container.ReadItemAsync<JsonObject>(name, partitionKey);

                //var view = (MaterialisedView)response.Resource.ToObject(type);
                var view = (MaterialisedView)JsonSerializer.Deserialize(response.Resource.ToJsonString(), type);

                return view;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return (MaterialisedView)Activator.CreateInstance(type);
            }
        }
    }
}