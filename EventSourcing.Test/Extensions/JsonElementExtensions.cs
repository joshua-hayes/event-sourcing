namespace EventSourcing.Test.Extensions;

using System.Text.Json;

public static class JsonElementExtensions
{
    // Extension method to mimic GetValue()
    public static JsonElement? GetValue(this JsonDocument document, string propertyName)
    {
        if (document.RootElement.TryGetProperty(propertyName, out JsonElement value))
        {
            return value;
        }
        return null;
    }

    public static T? GetValue<T>(this JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement value))
        {
            object result = null;

            if (typeof(T) == typeof(string))
            {
                result = value.GetString();
            }
            else if (typeof(T) == typeof(int) && value.TryGetInt32(out int intValue))
            {
                result = intValue;
            }
            // Add other types as needed, e.g., bool, double, etc.

            return (T?)result;
        }
        return default(T);
    }

    // Extension method to mimic Value<T>()
    public static T Value<T>(this JsonElement? element) where T : class
    {
        if (element.HasValue)
        {
            if (typeof(T) == typeof(string))
            {
                return element.Value.GetString() as T;
            }
            else if (typeof(T) == typeof(int) && element.Value.TryGetInt32(out int intValue))
            {
                return intValue as T;
            }
            // Add other types as needed, e.g., bool, double, etc.
        }
        return null;
    }

    public static T ValueOrDefault<T>(this JsonElement? element, T defaultValue) where T : struct
    {
        if (element.HasValue)
        {
            if (typeof(T) == typeof(int) && element.Value.TryGetInt32(out int intValue))
            {
                return (T)(object)intValue;
            }
            // Add other types as needed, e.g., bool, double, etc.
        }
        return defaultValue;
    }
}