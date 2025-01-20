namespace Eventum.Telemetry.Abstractions;

/// <summary>
/// Provides a unified interface for tracking telemetry data, including metrics and events.
/// </summary>
public interface ITelemetryProvider : IMetricTelemetryProvider,
                                      IEventTelemetryProvider
{
}