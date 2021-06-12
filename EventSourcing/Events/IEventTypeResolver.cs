using System;

namespace EventSourcing.Events
{

    /// <summary>
    /// Used to resolve an event type name from a string to a concrete even type that can be re-hydrated.
    /// </summary>
    public interface IEventTypeResolver
    {
        /// <summary>
        /// Resolves an event type name to a System.Type.
        /// </summary>
        /// <param name="typeName">The name of the event.</param>
        /// <returns>The <see cref="Type"/> that can be used to hydrate the provided type name</returns>
        Type Resolve(string typeName);
    }
}
