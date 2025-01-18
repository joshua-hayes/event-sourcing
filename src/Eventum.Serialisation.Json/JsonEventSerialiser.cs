using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection;
using Eventum.Serialisation.Json.Attributes;
using Eventum.Serialisation.Attributes;

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
                                                WriteIndented = false,
                                                Converters = { new IgnoreSerializationAttributeJsonConverter() }
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

            var properties = obj.GetType()
                                .GetProperties()
                                .Where(prop => prop.GetCustomAttribute<IgnoreSerializationAttribute>() == null)
                                .ToDictionary(prop => prop.Name, prop => prop.GetValue(obj));

            var jsonString = JsonSerializer.Serialize(obj, _options);

            return jsonString;
        }
    }
}
