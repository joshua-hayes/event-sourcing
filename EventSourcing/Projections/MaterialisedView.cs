using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventSourcing.Projections
{
    /// <summary>
    /// Maintains an up to date serialised version of a view that can be updated
    /// as <see cref="EventStreamChange"/>'s occur.
    /// </summary>
    public class MaterialisedView : IMaterialisedView
    {
        public MaterialisedView() : this(new JObject(), null)
        {
        }

        public MaterialisedView(JObject view, string etag)
        {
            View = view;
            Etag = etag;
        }

        /// <summary>
        /// <see cref="IMaterialisedView.View"/>
        /// </summary>
        public JObject View { get; set; }

        /// <summary>
        /// <see cref="IMaterialisedView.Etag"/>
        /// </summary>
        public string Etag { get; set; }
    }
}
