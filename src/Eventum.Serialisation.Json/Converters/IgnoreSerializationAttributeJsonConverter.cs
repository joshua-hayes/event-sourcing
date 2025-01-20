using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection;
using Eventum.Serialisation.Attributes;

namespace Eventum.Serialisation.Json.Attributes
{
    /// <summary>
    /// Provides a custom <see cref="JsonConverter{T}"/> that is <see cref="IgnoreSerializationAttribute"/> 'aware'.
    /// Ensures ignored properties are not de-serialised.
    /// </summary>
    public class IgnoreSerializationAttributeJsonConverter : JsonConverter<object>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            var convert = typeToConvert.GetProperties()
                                .Any(prop => prop.GetCustomAttribute<IgnoreSerializationAttribute>() != null);
            return convert;
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonObject = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);
            var result = Activator.CreateInstance(typeToConvert);

            foreach (var prop in typeToConvert.GetProperties()
                                              .Where(prop => prop.GetCustomAttribute<IgnoreSerializationAttribute>() == null))
            {
                var propValue = jsonObject[prop.Name];
                if (propValue is JsonElement jsonElement)
                {
                    prop.SetValue(result, jsonElement.Deserialize(prop.PropertyType, options));
                }
                else
                {
                    prop.SetValue(result, propValue);
                }
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            var type = value.GetType();
            var dictionary = type.GetProperties()
                                 .Where(prop => prop.GetCustomAttribute<IgnoreSerializationAttribute>() == null)
                                 .ToDictionary(prop => prop.Name, prop => prop.GetValue(value));

            JsonSerializer.Serialize(writer, dictionary, options);
        }
    }
}
