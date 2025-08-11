using Dynastream.Fit;
using RunTracker.Models;
using DateTime = System.DateTime;

public static class FitToRunConverter
{
    public static Run? ExtractRunFromFitFile(string fitFilePath)
    {
        var (sessionMessage, activityMessage) = ParseFitFile(fitFilePath);
        
        if (sessionMessage == null)
        {
            return null;
        }

        return CreateRunFromMessages(sessionMessage, activityMessage);
    }

    private static (SessionMesg?, ActivityMesg?) ParseFitFile(string fitFilePath)
    {
        var decoder = new Decode();
        SessionMesg? sessionMessage = null;
        ActivityMesg? activityMessage = null;

        decoder.MesgEvent += (sender, e) =>
        {
            if (e.mesg.Name == "Session")
            {
                sessionMessage = new SessionMesg(e.mesg);
            }
            else if (e.mesg.Name == "Activity")
            {
                activityMessage = new ActivityMesg(e.mesg);
            }
        };

        using var fitStream = new FileStream(fitFilePath, FileMode.Open, FileAccess.Read);
        
        if (!decoder.IsFIT(fitStream))
        {
            throw new InvalidOperationException("Not a valid FIT file");
        }

        decoder.Read(fitStream);
        return (sessionMessage, activityMessage);
    }

    private static Run CreateRunFromMessages(SessionMesg sessionMessage, ActivityMesg? activityMessage)
    {
        var distanceKm = (sessionMessage.GetTotalDistance() ?? 0) / 1000f;
        var durationSeconds = sessionMessage.GetTotalTimerTime() ?? 0;
        var duration = TimeSpan.FromSeconds(durationSeconds);
        var averageHeartRate = (int)(sessionMessage.GetAvgHeartRate() ?? 0);
        var averagePace = distanceKm > 0 ? TimeSpan.FromSeconds(durationSeconds / distanceKm) : TimeSpan.Zero;
        
        var startTime = activityMessage?.GetTimestamp()?.GetDateTime() ?? 
                       sessionMessage.GetStartTime()?.GetDateTime() ?? 
                       DateTime.UtcNow;

        return new Run
        {
            Id = Guid.NewGuid(),
            StartTime = startTime,
            Duration = duration,
            DistanceKm = distanceKm,
            AverageHeartRate = averageHeartRate,
            AveragePace = averagePace
        };
    }
}