using OpenTelemetry;
using OpenTelemetry.Metrics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;

namespace Eventum.Telemetry.OpenTelemetry
{
    /// <summary>
    /// Provides an implementation of the Eventum telemetry abstraction using OpenTelemetry.
    /// This class enables tracking of metrics, events, and exceptions in a simplified manner whilst leveraging the power and flexibility of
    /// OpenTelemetry providers.
    /// 
    /// Usage:
    /// 
    /// <code>
    /// var telemetryProvider = new OpenTelemetryTelemetryProvider(TelemetryVerbosity.Info);
    /// 
    /// telemetryProvider.TrackMetric("ExampleMetric", 100);
    /// telemetryProvider.TrackEvent("ExampleEvent", new Dictionary&lt;string, string&gt; { { "key", "value" } });
    /// telemetryProvider.TrackException(new Exception("ExampleException"));
    /// </code>
    /// 
    /// Clients can further customise their OpenTelemetry configuration by adding specific exporters and other settings to suit their needs.
    /// 
    /// <remarks>
    /// This implementation uses OpenTelemetry's <see cref="Meter"/> for metrics and <see cref="ActivitySource"/> for events and exceptions.
    /// Clients are responsible for configuring and building their own <see cref="TracerProvider"/> and <see cref="MeterProvider"/>
    /// with desired exporters.
    /// </remarks>
    /// </summary>
    public class OpenTelemetryTelemetryProvider : ITelemetryProvider
    {
        private readonly IMeterWrapper _meter;
        private readonly IActivitySourceWrapper _activitySource;
        private readonly TelemetryVerbosity _currentVerbosity;

        /// <summary>
        /// Initialises a new instance of the <see cref="OpenTelemetryTelemetryProvider"/> class.
        /// </summary>
        /// <param name="verbosity">The verbosity level for telemetry tracking.</param>
        public OpenTelemetryTelemetryProvider(TelemetryVerbosity verbosity) : this(verbosity,
                                                                                   new MeterWrapper(new Meter("Eventum.Telemetry.Metrics", "1.0.0")),
                                                                                   new ActivitySourceWrapper(new ActivitySource("Eventum.Telemetry", "1.0.0")))
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="OpenTelemetryTelemetryProvider"/> class with specified meter and activity source.
        /// </summary>
        /// <param name="verbosity">The verbosity level for telemetry tracking.</param>
        /// <param name="meter">The meter wrapper for tracking metrics.</param>
        /// <param name="activitySource">The activity source wrapper for tracking events and exceptions.</param>
        public OpenTelemetryTelemetryProvider(TelemetryVerbosity verbosity, IMeterWrapper meter, IActivitySourceWrapper activitySource)
        {
            _meter = meter;
            _activitySource = activitySource;
            _currentVerbosity = verbosity;
        }

        /// <summary>
        /// Whether or not tracking should occur.
        /// </summary>
        /// <param name="verbosity">The requested verbosity level to consider tracking.</param>
        /// <returns>True if tracking should occur, false otherwise.</returns>
        private bool ShouldTrack(TelemetryVerbosity verbosity)
        {
            return verbosity >= _currentVerbosity;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method uses OpenTelemetry to track a numerical metric.
        /// The metric is recorded only if the specified verbosity level meets or exceeds the current verbosity setting.
        /// </remarks>
        public void TrackMetric(string name, double value, IDictionary<string, string> properties = null, TelemetryVerbosity verbosity = TelemetryVerbosity.Info)
        {
            if (ShouldTrack(verbosity))
            {
                var counter = _meter.CreateCounter(name);
                var attributes = properties?.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value)).ToArray();
                counter.Add(value, attributes);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method uses OpenTelemetry to start an activity for tracking an event.
        /// The event is recorded only if the specified verbosity level meets or exceeds the current verbosity setting.
        /// </remarks>
        public void TrackEvent(string name, IDictionary<string, string> properties = null, TelemetryVerbosity verbosity = TelemetryVerbosity.Info)
        {
            if (ShouldTrack(verbosity))
            {
                using var activity = _activitySource.StartActivity(name);
                if (properties != null)
                {
                    foreach (var prop in properties)
                    {
                        activity?.SetTag(prop.Key, prop.Value);
                    }
                }
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method uses OpenTelemetry to start an activity for tracking an exception.
        /// The exception is recorded only if the specified verbosity level meets or exceeds the current verbosity setting.
        /// </remarks>
        public void TrackException(Exception exception, IDictionary<string, string> properties = null, TelemetryVerbosity verbosity = TelemetryVerbosity.Error)
        {
            if (ShouldTrack(verbosity))
            {
                using var activity = _activitySource.StartActivity("Exception");
                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.SetTag("exception", exception.ToString());

                if (properties != null)
                {
                    foreach (var prop in properties)
                    {
                        activity?.SetTag(prop.Key, prop.Value);
                    }
                }
            }
        }
    }
}
