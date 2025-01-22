namespace Eventum.Telemetry;

/// <summary>
/// Provides a unified interface for tracking telemetry data, including metrics and events.
/// </summary>
public interface ITelemetryProvider : IMetricTelemetryProvider,
                                      IEventTelemetryProvider,
                                      IExceptionTelemetryProvider
{
}