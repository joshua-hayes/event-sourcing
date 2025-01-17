namespace Eventum.Test.Extensions;

using System;
using System.Text.Json;

public class JsonDocumentWrapper : IEquatable<JsonDocumentWrapper>
{
    public JsonDocument JsonDocument { get; }

    public JsonDocumentWrapper(JsonDocument jsonDocument)
    {
        JsonDocument = jsonDocument ?? throw new ArgumentNullException(nameof(jsonDocument));
    }

    public bool Equals(JsonDocumentWrapper other)
    {
        if (other == null) return false;
        return JsonDocument.RootElement.ToString() == other.JsonDocument.RootElement.ToString();
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as JsonDocumentWrapper);
    }

    public override int GetHashCode()
    {
        return JsonDocument.RootElement.ToString().GetHashCode();
    }

    public static bool operator ==(JsonDocumentWrapper left, JsonDocumentWrapper right)
    {
        if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
        return left.Equals(right);
    }

    public static bool operator !=(JsonDocumentWrapper left, JsonDocumentWrapper right)
    {
        return !(left == right);
    }
}
