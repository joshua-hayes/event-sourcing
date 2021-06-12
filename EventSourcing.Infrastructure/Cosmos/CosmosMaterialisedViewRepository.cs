using EventSourcing.Projections;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EventSourcing.Infrastructure.Cosmos
{
    /// <summary>
    /// Stores and saves <see cref="MaterialisedView"/>'s  in a cosomos view container.
    /// </summary>
    public class CosmosMaterialisedViewRepository : IMaterialisedViewRepository
    {
        private readonly CosmosClient _client;
        private readonly string _databaseId;
        private readonly string _containerId;

        public CosmosMaterialisedViewRepository(string endpointUrl,
                                                string authorizationKey,
                                                string databaseId,
                                                string containerId = "views")
        {
            _client = new CosmosClient(endpointUrl, authorizationKey);

            _databaseId = databaseId;
            _containerId = containerId;
        }

        /// <summary>
        /// <see cref="IMaterialisedViewRepository.LoadViewAsync{TView}(string)"/>
        /// </summary>
        public async Task<bool> SaveViewAsync(string name, MaterialisedView view)
        {
            var container = _client.GetContainer(_databaseId, _containerId);
            var partitionKey = new PartitionKey(name);

            var payload = JObject.FromObject(view);
            payload.Remove("view");
            payload.Remove("_etag");


            try
            {
                var viewData = new {
                    id = name,
                    view = view
                };
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
            var container = _client.GetContainer(_databaseId, _containerId);
            var partitionKey = new PartitionKey(name);

            try
            {
                var response = await container.ReadItemAsync<TView>(name, partitionKey);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return new TView();
            }
        }
    }
}