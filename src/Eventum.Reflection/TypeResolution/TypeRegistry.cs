namespace Eventum.Reflection.TypeResolution;

/// <summary>
/// Provides a mechanism to register types that are not known at compile time.
/// </summary>
public static class TypeRegistry
{
    /// <summary>
    /// Registers the specified type with the current <see cref="AppDomain"/>.
    /// </summary>
    /// <typeparam name="T">The type whose assembly should be registered.</typeparam>
    public static void Register<T>()
    {
        var assembly = typeof(T).Assembly;
        if (!AppDomain.CurrentDomain.GetAssemblies().Contains(assembly))
        {
            AppDomain.CurrentDomain.Load(assembly.FullName);
        }
    }
}