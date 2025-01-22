using Eventum.Telemetry.OpenTelemetry;
using System.Diagnostics.Metrics;

namespace Eventum.Telemetry.OpenTelemetry;

/// <summary>
/// Provides an implementation of the ICounterWrapper interface, wrapping the OpenTelemetry <see cref="Counter"/> class.
/// </summary>
/// <remarks>
/// This is required due to sealed classes in the OpenTelemetry library that are not easily unit tested and mocked.
/// </remarks>
public class CounterWrapper : ICounterWrapper
{
    private readonly Counter<double> _counter;

    /// <summary>
    /// Initialises a new instance of the CounterWrapper class.
    /// </summary>
    /// <param name="counter">The Counter instance to wrap.</param>
    public CounterWrapper(Counter<double> counter)
    {
        _counter = counter;
    }

    /// <inheritdoc />
    public void Add(double value, KeyValuePair<string, object>[] attributes = null)
    {
        _counter.Add(value, attributes);
    }
}
