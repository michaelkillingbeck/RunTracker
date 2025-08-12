using Dynastream.Fit;
using RunTracker.Models;
using DateTime = System.DateTime;

namespace RunTracker.FitParser;

public static class FitToRunConverter
{
    public static Run? ExtractRunFromFitFile(string fitFilePath)
    {
        var visitor = new RunDataVisitor();
        FitMessageExtractor.ExtractMessages(fitFilePath, visitor);

        if (visitor.SessionMessage == null || visitor.ActivityMessage == null)
        {
            return null;
        }

        return Create(visitor);
    }

    public static Run Create(RunDataVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        var distanceKm = CalculateDistance(visitor);
        var durationSeconds = visitor.SessionMessage!.GetTotalTimerTime() ?? 0;
        var duration = TimeSpan.FromSeconds(durationSeconds);
        var averageHeartRate = CalculateAverageHeartRate(visitor);
        var averagePace = distanceKm > 0 ? TimeSpan.FromSeconds(durationSeconds / distanceKm) : TimeSpan.Zero;
        var startTime = visitor.ActivityMessage!.GetTimestamp()?.GetDateTime() ??
                       visitor.SessionMessage.GetStartTime()?.GetDateTime() ??
                       DateTime.UtcNow;

        return new Run
        {
            Id = Guid.NewGuid(),
            StartTime = startTime,
            Duration = duration,
            DistanceKm = distanceKm,
            AverageHeartRate = averageHeartRate,
            AveragePace = averagePace,
        };
    }

    private static float CalculateDistance(RunDataVisitor visitor)
    {
        // Try to get distance from Record messages first
        if (visitor.FinalDistance > 0)
        {
            return visitor.FinalDistance / 1000f; // Convert from meters to kilometers
        }

        // Fall back to Session message distance
        return (visitor.SessionMessage!.GetTotalDistance() ?? 0) / 1000f;
    }

    private static int CalculateAverageHeartRate(RunDataVisitor visitor)
    {
        // Try to get average heart rate from Record messages first
        if (visitor.AverageHeartRate > 0)
        {
            return (int)visitor.AverageHeartRate;
        }

        // Fall back to Session message average heart rate
        return (int)(visitor.SessionMessage!.GetAvgHeartRate() ?? 0);
    }
}