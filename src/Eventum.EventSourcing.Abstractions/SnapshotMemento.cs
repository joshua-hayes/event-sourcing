using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Eventum.EventSourcing.Test")]
namespace Eventum.EventSourcing
{
    /// <summary>
    /// Provides the ability for an originator to restore its internal state to a previous state.
    /// </summary>
    public class SnapshotMemento
    {
        private IDictionary<string, object> _state;

        public SnapshotMemento(object state)
        {
            _state = state.GetType()
                          .GetProperties()
                          .ToDictionary(prop => prop.Name, prop => prop.GetValue(state));
        }

        /// <summary>
        // Gets the originator's state by property name.
        /// </summary>
        /// <remarks>
        /// Scope restricted to ensure state remains private and well encapsulated (with the exception
        /// of access by the  test library)
        /// </remarks>
        internal object GetState(string propertyName)
        {
            if (_state.TryGetValue(propertyName, out object value))
                return value;

            return null;
        }
    }
}