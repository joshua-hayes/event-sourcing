namespace Eventum.Telemetry.OpenTelemetry;

/// <summary>
/// Provides a wrapper for the Counter class to facilitate mocking and unit testing.
/// </summary>
public interface ICounterWrapper
{
    /// <summary>
    /// Adds a value to the counter with optional attributes.
    /// </summary>
    /// <param name="value">The value to add to the counter.</param>
    /// <param name="attributes">Optional attributes associated with the counter.</param>
    void Add(double value, KeyValuePair<string, object>[] attributes = null);
}
