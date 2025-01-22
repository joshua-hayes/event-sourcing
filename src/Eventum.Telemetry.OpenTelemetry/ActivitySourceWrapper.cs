using Eventum.Telemetry.OpenTelemetry;
using System.Diagnostics;

namespace Eventum.Telemetry.OpenTelemetry;

/// <summary>
/// Provides an implementation of the IActivitySourceWrapper interface, wrapping the OpenTelemetry <see cref="ActivitySource"/> class.
/// </summary>
/// <remarks>
/// This is required due to sealed classes in the OpenTelemetry library that are not easily unit tested and mocked.
/// </remarks>
public class ActivitySourceWrapper : IActivitySourceWrapper
{
    private readonly ActivitySource _activitySource;

    /// <summary>
    /// Initialises a new instance of the ActivitySourceWrapper class.
    /// </summary>
    /// <param name="activitySource">The ActivitySource instance to wrap.</param>
    public ActivitySourceWrapper(ActivitySource activitySource)
    {
        _activitySource = activitySource;
    }

    /// <inheritdoc />
    public Activity StartActivity(string name)
    {
        return _activitySource.StartActivity(name);
    }
}
