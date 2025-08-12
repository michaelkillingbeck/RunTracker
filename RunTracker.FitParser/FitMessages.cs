using Dynastream.Fit;

namespace RunTracker.FitParser;

public class FitMessages
{
    public ActivityMesg ActivityMessage { get; }

    public SessionMesg SessionMessage { get; }

    public RecordMesg? RecordMessage { get; }

    public FitMessages(ActivityMesg activityMessage, SessionMesg sessionMessage, RecordMesg? recordMessage = null)
    {
        ActivityMessage = activityMessage;
        SessionMessage = sessionMessage;
        RecordMessage = recordMessage;
    }
}