using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventSourcing.Projections
{
    /// <summary>
    /// Stores state in the form of a view that can be hydrated to apply further changes.
    /// </summary>
    public interface IMaterialisedView
    {
        /// <summary>
        /// The serialised view.
        /// </summary>
        [JsonProperty("view")]
        JObject View { get; set; }

        /// <summary>
        /// The entity tag associated with the view that can be used for
        /// optimistic concurrency control.
        /// </summary>
        [JsonProperty("_etag")]
        public string Etag { get; set; }
    }
}
