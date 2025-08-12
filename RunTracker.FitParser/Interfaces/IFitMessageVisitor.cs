using Dynastream.Fit;

namespace RunTracker.FitParser.Interfaces;

public interface IFitMessageVisitor
{
    void VisitRecord(RecordMesg record);

    void VisitSession(SessionMesg session);

    void VisitActivity(ActivityMesg activity);

    void OnExtractionComplete();
}