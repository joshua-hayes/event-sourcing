namespace Eventum.Telemetry.Abstractions
{
    /// <summary>
    /// Provides functionality to track event data for telemetry purposes.
    /// </summary>
    public interface IEventTelemetryProvider
    {
        /// <summary>
        /// Tracks an event with an optional set of properties.
        /// </summary>
        /// <param name="name">The name of the event to track.</param>
        /// <param name="properties">Optional additional properties associated with the event.</param>
        void TrackEvent(string name, IDictionary<string, string> properties = null);
    }

}
