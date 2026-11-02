namespace TaskTrackerCli.Application;

/// <summary>Abstraction over "now" so time-dependent logic is testable.</summary>
public interface IClock
{
    DateTimeOffset Now { get; }
}
