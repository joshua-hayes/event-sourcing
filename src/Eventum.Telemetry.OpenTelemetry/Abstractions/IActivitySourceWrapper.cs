using System.Diagnostics;

namespace Eventum.Telemetry.OpenTelemetry;

/// <summary>
/// Provides a wrapper for the ActivitySource class to facilitate mocking and unit testing.
/// </summary>
public interface IActivitySourceWrapper
{
    /// <summary>
    /// Starts an activity with the specified name.
    /// </summary>
    /// <param name="name">The name of the activity.</param>
    /// <returns>An Activity instance.</returns>
    Activity StartActivity(string name);
}
