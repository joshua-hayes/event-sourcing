using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace EventSourcing.Projection
{
    /// <summary>
    /// Maintains an up to date serialised version of a view that can be updated
    /// as <see cref="EventStreamChange"/>'s occur.
    /// </summary>
    public class MaterialisedView : IMaterialisedView
    {
        public MaterialisedView() : this(JsonDocument.Parse("{}"), null, new List<string>())
        {
        }

        public MaterialisedView(JsonDocument view, string etag, IList<string> changeset)
        {
            Etag = etag;
            Changeset = changeset;
            View = view;
        }

        /// <summary>
        /// <see cref="IMaterialisedView.Etag"/>
        /// </summary>
        public string Etag { get; set; }

        /// <summary>
        /// <see cref="IMaterialisedView.Changeset"/>
        /// </summary>
        public IList<string> Changeset { get; set; }

        /// <summary>
        /// <see cref="IMaterialisedView.View"/>
        /// </summary>
        public JsonDocument View { get; set; }

        /// <summary>
        /// Serialises the view to Json.
        /// </summary>
        public void Serialise()
        {
            var properties = this.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(this));

            // Remove unwanted properties
            properties.Remove(nameof(View));
            properties.Remove(nameof(Etag));
            properties.Remove(nameof(Changeset));

            var jsonString = JsonSerializer.Serialize(properties,
                                                      new JsonSerializerOptions
                                                      {
                                                          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                                          WriteIndented = true
                                                      });


            this.View = JsonDocument.Parse(jsonString);
        }
    }
}
