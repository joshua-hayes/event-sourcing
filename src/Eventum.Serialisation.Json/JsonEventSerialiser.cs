using Eventum.Serialisation.Abstractions;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Eventum.Serialisation.Json
{
    /// <summary>
    /// Json serialiser.
    /// </summary>
    public class JsonEventSerialiser : IEventSerialiser
    {
        private JsonSerializerOptions _options;

        public JsonEventSerialiser() : this(new JsonSerializerOptions {
                                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                                WriteIndented = false
                                            })
        { 
        }

        public JsonEventSerialiser(JsonSerializerOptions options)
        {
            _options = options;
        }

        public JsonSerializerOptions Options => _options;

        /// <summary>
        /// <see cref="IEventSerialiser.Serialise{T}(T)"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public T Deserialise<T>(string data)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException("data");

            return JsonSerializer.Deserialize<T>(data!, _options);
        }

        /// <summary>
        /// <see cref="IEventSerialiser.Deserialise{T}(string)"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public string Serialise<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var properties = obj.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(obj));
            var jsonString = JsonSerializer.Serialize(properties, _options);

            return jsonString;
        }
    }
}
