namespace RunTracker.Models;

public class Run
{
    public int AverageHeartRate { get; init; }

    public TimeSpan AveragePace { get; init; }

    public float DistanceKm { get; init; }

    public TimeSpan Duration { get; init; }

    public Guid Id { get; init; }

    public DateTime StartTime { get; init; }
}
