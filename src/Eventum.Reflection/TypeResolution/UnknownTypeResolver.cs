using System.Reflection;

namespace Eventum.Reflection.TypeResolution;

/// <summary>
/// Provides dynamic type resolution for types that are not known at compile time by scanning all assemblies in
/// the current <see cref="AppDomain"/>.
/// </summary>
public class UnknownTypeResolver : ITypeResolver
{
    private readonly Dictionary<string, Type> _typeCache;
    private readonly Assembly[] _assemblies;

    public UnknownTypeResolver()
    {
        _typeCache = new Dictionary<string, Type>();
        _assemblies = AppDomain.CurrentDomain.GetAssemblies();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Scans all assemblies in the current <see cref="AppDomain"/> for the specified type name,
    /// caching the resolved <see cref="Type"/> for future lookups.
    /// </remarks>
    public Type Resolve(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
            throw new ArgumentNullException(nameof(typeName));

        if (_typeCache.TryGetValue(typeName, out Type cachedType))
            return cachedType;

        foreach (var assembly in _assemblies)
        {    
            var resolvedType = assembly.DefinedTypes.SingleOrDefault(a => a.Name == typeName);
            if (resolvedType != null)
            {
                _typeCache.Add(typeName, resolvedType);
                return resolvedType;
            }       
        }
        
        throw new TypeLoadException($"Type '{typeName}' could not be resolved.");
    }
}