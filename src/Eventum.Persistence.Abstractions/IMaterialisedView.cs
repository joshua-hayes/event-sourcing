using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eventum.Persistence
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
        public IList<string> Changeset { get; set; }

        /// <summary>
        /// The serialised view.
        /// </summary>
        string View { get; set; }

        /// <summary>
        /// The entity tag associated with the view that can be used for
        /// optimistic concurrency control.
        /// </summary>
        public string Etag { get; set; }
    }
}
