namespace Eventum.Telemetry
{
    /// <summary>
    /// Provides functionality to track exceptions for telemetry purposes.
    /// </summary>
    public interface IExceptionTelemetryProvider
    {
        /// <summary>
        /// Tracks an exception with an optional set of properties.
        /// </summary>
        /// <param name="name">The name of the exception to track.</param>
        /// <param name="properties">Optional additional properties associated with the event.</param>
        /// <param name="verbosity">The verbosity level of the exception.</param>
        void TrackEvent(string name,
                        IDictionary<string, string> properties = null,
                        TelemetryVerbosity verbosity = TelemetryVerbosity.Info);
    }
}
