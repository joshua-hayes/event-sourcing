namespace Eventum.Telemetry
{
    /// <summary>
    /// Defines the verbosity levels for telemetry tracking.
    /// </summary>
    public enum TelemetryVerbosity
    {
        /// <summary>
        /// Debug level, captures detailed and verbose telemetry.
        /// Suitable for development and debugging scenarios.
        /// </summary>
        Debug,

        /// <summary>
        /// Information level, captures general informational telemetry.
        /// Suitable for regular monitoring and operational insights.
        /// </summary>
        Info,

        /// <summary>
        /// Warning level, captures potential issues that might need attention.
        /// Suitable for identifying trends that could lead to errors.
        /// </summary>
        Warning,

        /// <summary>
        /// Error level, captures critical errors and failures.
        /// Suitable for alerting and immediate attention in production.
        /// </summary>
        Error
    }
}