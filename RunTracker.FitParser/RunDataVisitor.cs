using Dynastream.Fit;
using RunTracker.FitParser.Interfaces;
using DateTime = System.DateTime;

namespace RunTracker.FitParser;

public class RunDataVisitor : IFitMessageVisitor
{
    private SessionMesg? sessionMessage;
    private ActivityMesg? activityMessage;
    private float finalDistance;
    private DateTime? lastRecordTimestamp;
    private int heartRateSum;
    private int heartRateCount;

    public SessionMesg? SessionMessage => sessionMessage;

    public ActivityMesg? ActivityMessage => activityMessage;

    public float AverageHeartRate => heartRateCount > 0 ? (float)heartRateSum / heartRateCount : 0;

    public float FinalDistance => finalDistance;

    public void VisitRecord(RecordMesg record)
    {
        ArgumentNullException.ThrowIfNull(record);
        var timestamp = record.GetTimestamp()?.GetDateTime();

        // Track final distance from the latest record
        if (timestamp.HasValue && (!lastRecordTimestamp.HasValue || timestamp > lastRecordTimestamp))
        {
            var distance = record.GetDistance();
            if (distance.HasValue)
            {
                finalDistance = distance.Value;
                lastRecordTimestamp = timestamp;
            }
        }

        // Accumulate heart rate data from all records
        var heartRate = record.GetHeartRate();
        if (heartRate.HasValue)
        {
            heartRateSum += heartRate.Value;
            heartRateCount++;
        }
    }

    public void VisitSession(SessionMesg session)
    {
        ArgumentNullException.ThrowIfNull(session);
        sessionMessage = session;
    }

    public void VisitActivity(ActivityMesg activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
        activityMessage = activity;
    }

    public void OnExtractionComplete()
    {
        // Any final processing if needed
    }
}