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
        /// <param name="exception">The exception to track.</param>
        /// <param name="properties">Optional additional properties associated with the event.</param>
        /// <param name="verbosity">The verbosity level of the exception.</param>
        void TrackException(Exception exception,
                            IDictionary<string, string> properties = null,
                            TelemetryVerbosity verbosity = TelemetryVerbosity.Error);
    }
}
