using TaskTrackerCli.Application;

namespace TaskTrackerCli.Infrastructure;

/// <summary>Wall-clock UTC time, truncated to whole seconds to keep the JSON tidy.</summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset Now
    {
        get
        {
            var now = DateTimeOffset.UtcNow;
            return new DateTimeOffset(now.Ticks - now.Ticks % TimeSpan.TicksPerSecond, TimeSpan.Zero);
        }
    }
}
