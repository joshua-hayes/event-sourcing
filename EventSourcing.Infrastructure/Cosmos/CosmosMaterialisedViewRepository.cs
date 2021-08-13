using EventSourcing.Projections;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;

namespace EventSourcing.Cosmos
{
    /// <summary>
    /// Stores and saves <see cref="MaterialisedView"/>'s  in a cosomos view container.
    /// </summary>
    public class CosmosMaterialisedViewRepository : IMaterialisedViewRepository
    {
        private readonly CosmosClient _client;
        private readonly string _databaseId;
        private readonly string _containerId;

        public CosmosMaterialisedViewRepository(CosmosClient client,
                                                string databaseId,
                                                string containerId = "views")
        {
            _client = client;
            _databaseId = databaseId;
            _containerId = containerId;
        }

        /// <summary>
        /// <see cref="IMaterialisedViewRepository.LoadViewAsync{TView}(string)"/>
        /// </summary>
        public async Task<bool> SaveViewAsync(string name, MaterialisedView view)
        {
            try
            {
                var payload = JObject.FromObject(view);
                payload.Remove("view");
                payload.Remove("_etag");

                var viewData = new
                {
                    id = name,
                    view = view
                };

                var partitionKey = new PartitionKey(name);
                var container = _client.GetContainer(_databaseId, _containerId);
                await container.UpsertItemAsync(viewData,
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

            try
            {
                var partitionKey = new PartitionKey(name);
                var container = _client.GetContainer(_databaseId, _containerId);
                var response = await container.ReadItemAsync<TView>(name, partitionKey);

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
            try
            {
                var partitionKey = new PartitionKey(name);
                var container = _client.GetContainer(_databaseId, _containerId);
                var response = await container.ReadItemAsync<JObject>(name, partitionKey);

                var view = (MaterialisedView)response.Resource.ToObject(type);

                return view;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return (MaterialisedView)Activator.CreateInstance(type);
            }
        }
    }
}