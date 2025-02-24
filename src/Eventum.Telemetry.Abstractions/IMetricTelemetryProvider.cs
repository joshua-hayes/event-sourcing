﻿namespace Eventum.Telemetry
{
    /// <summary>
    /// Provides functionality to track metric data for telemetry purposes.
    /// </summary>
    public interface IMetricTelemetryProvider
    {
        /// <summary>
        /// Tracks a numerical metric with an optional set of properties.
        /// </summary>
        /// <param name="name">The name of the metric to track.</param>
        /// <param name="value">The value of the metric.</param>
        /// <param name="properties">Optional additional properties associated with the metric.</param>
        /// <param name="verbosity">The verbosity level of the metric.</param>
        void TrackMetric(string name,
                         double value,
                         IDictionary<string, string> properties = null,
                         TelemetryVerbosity verbosity = TelemetryVerbosity.Info);
    }
}
