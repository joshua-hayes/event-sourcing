using System.Diagnostics.Metrics;

namespace Eventum.Telemetry.OpenTelemetry;

/// <summary>
/// Provides a wrapper for the Meter class to facilitate mocking and unit testing.
/// </summary>
public interface IMeterWrapper
{
    /// <summary>
    /// Creates a counter with the specified name.
    /// </summary>
    /// <param name="name">The name of the counter.</param>
    /// <returns>A wrapper for the Counter for tracking metrics.</returns>
    ICounterWrapper CreateCounter(string name);
}
