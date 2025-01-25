namespace Eventum.Reflection.TypeResolution;

/// <summary>
/// Used to resolve a <see cref="Type"/> from a string.
/// </summary>
/// <remarks>
/// Eventum uses this interface to resolve de-serialised events to a concrete event type.
/// </remarks>
public interface ITypeResolver
{
    /// <summary>
    /// Resolves the specified type name to a <see cref="Type"/>.
    /// </summary>
    /// <param name="typeName">The name of the type to resolve.</param>
    /// <returns>The <see cref="Type"/>, if resolved.</returns>
    Type Resolve(string typeName);
}