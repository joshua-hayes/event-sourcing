namespace Eventum.Reflection.TypeResolution;

/// <inheritdoc />
public class KnownTypeResolver : ITypeResolver
{
    private readonly string _assemblyName;
    private readonly string _namespace;

    public KnownTypeResolver(string assemblyName, string @namespace)
    {
        _assemblyName = assemblyName;
        _namespace = @namespace;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"></exception>
    public Type Resolve(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
            throw new ArgumentNullException(nameof(typeName));

        var fullyQualifiedNamespace = $"{_namespace}.{typeName}, {_assemblyName}";
        var type = Type.GetType(fullyQualifiedNamespace);


        if (type == null)
            throw new TypeLoadException($"Type '{fullyQualifiedNamespace}' could not be resolved.");

        return type;
    }
}
