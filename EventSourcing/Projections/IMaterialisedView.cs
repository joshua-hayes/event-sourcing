using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace EventSourcing.Projections
{
    /// <summary>
    /// Stores state in the form of a view that can be hydrated to apply further changes.
    /// </summary>
    public interface IMaterialisedView
    {
        /// <summary>
        /// Represents the collection of event changes that were applied to arrive
        /// at the current state of the view.
        /// </summary>
        [JsonProperty("changeset")]
        public IList<string> Changeset { get; set; }

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
