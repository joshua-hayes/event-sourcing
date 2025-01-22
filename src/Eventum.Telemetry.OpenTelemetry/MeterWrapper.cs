using System.Diagnostics.Metrics;

namespace Eventum.Telemetry.OpenTelemetry;

/// <summary>
/// Provides an implementation of the IMeterWrapper interface, wrapping the OpenTelemetry <see cref="Meter"/> class.
/// </summary>
/// <remarks>
/// This is required due to sealed classes in the OpenTelemetry library that are not easily unit tested and mocked.
/// </remarks>
public class MeterWrapper : IMeterWrapper
{
    private readonly Meter _meter;

    /// <summary>
    /// Initialises a new instance of the MeterWrapper class.
    /// </summary>
    /// <param name="meter">The Meter instance to wrap.</param>
    public MeterWrapper(Meter meter)
    {
        _meter = meter;
    }

    /// <inheritdoc />
    public ICounterWrapper CreateCounter(string name)
    {
        var counter = _meter.CreateCounter<double>(name);
        return new CounterWrapper(counter);
    }
}
