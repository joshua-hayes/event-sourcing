using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        [JsonPropertyName("changeset")]
        public IList<string> Changeset { get; set; }

        /// <summary>
        /// The serialised view.
        /// </summary>
        [JsonPropertyName("view")]
        JsonDocument View { get; set; }

        /// <summary>
        /// The entity tag associated with the view that can be used for
        /// optimistic concurrency control.
        /// </summary>
        [JsonPropertyName("_etag")]
        public string Etag { get; set; }
    }
}
