namespace Eventum.Serialisation.Attributes;

/// <summary>
/// Provides a serialisation agnostic attribute for flagging properties to ignore.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IgnoreSerializationAttribute : Attribute
{
}
