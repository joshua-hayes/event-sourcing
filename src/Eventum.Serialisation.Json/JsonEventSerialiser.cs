using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection;
using Eventum.Serialisation.Json.Attributes;
using Eventum.Serialisation.Attributes;
using System.Diagnostics;
using Eventum.Telemetry;

namespace Eventum.Serialisation.Json
{
    /// <summary>
    /// Json serialiser.
    /// </summary>
    public class JsonEventSerialiser : IEventSerialiser
    {
        private JsonSerializerOptions _options;
        private ITelemetryProvider _telemetryProvider;

        public JsonEventSerialiser(ITelemetryProvider telemetryProvider) : this(new JsonSerializerOptions {
                                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                                WriteIndented = false,
                                                Converters = { new IgnoreSerializationAttributeJsonConverter() },
                                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                           }, telemetryProvider)
        { 
        }

        public JsonEventSerialiser(JsonSerializerOptions options, ITelemetryProvider telemetryProvider)
        {
            _options = options;
            _telemetryProvider = telemetryProvider;
        }

        public JsonSerializerOptions Options => _options;

        /// <summary>
        /// <see cref="IEventSerialiser.Serialise{T}(T)"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public T Deserialise<T>(string data)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                if (string.IsNullOrEmpty(data))
                    throw new ArgumentNullException("data");

                var result = JsonSerializer.Deserialize<T>(data!, _options);
                _telemetryProvider.TrackMetric("JsonEventSerialiser.Deserialise.Time", stopwatch.ElapsedMilliseconds);

                return result;
            } catch (Exception ex)
            {
                _telemetryProvider.TrackException(ex, new Dictionary<string, string>
                {
                    { "Operation", "Deserialise" },
                    { "Data", data },
                    { "ErrorMessage", ex.Message },
                    { "StackTrace", ex.StackTrace},
                });
                throw;
            }
        }

        /// <summary>
        /// <see cref="IEventSerialiser.Deserialise{T}(string)"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public string Serialise<T>(T obj)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                if (obj == null)
                    throw new ArgumentNullException("obj");

                var properties = obj.GetType()
                                    .GetProperties()
                                    .Where(prop => prop.GetCustomAttribute<IgnoreSerializationAttribute>() == null)
                                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(obj));

                var jsonString = JsonSerializer.Serialize(obj, _options);
                _telemetryProvider.TrackMetric("JsonEventSerialiser.Serialise.Time", stopwatch.ElapsedMilliseconds);

                return jsonString;
            }catch (Exception ex)
            {
                _telemetryProvider.TrackException(ex, new Dictionary<string, string>
                {
                    { "Operation", nameof(Serialise)},
                    { "ErrorMessage", ex.Message },
                    { "StackTrace", ex.StackTrace},
                });
                throw;
            }
        }
    }
}
