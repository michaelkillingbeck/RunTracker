using Dynastream.Fit;
using RunTracker.FitParser.Interfaces;
using DateTime = System.DateTime;

namespace RunTracker.FitParser;

public static class FitMessageExtractor
{
    public static void ExtractMessages(string fitFilePath, IFitMessageVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        var decoder = new Decode();

        decoder.MesgEvent += (sender, e) =>
        {
            if (e.mesg.Name == "Session")
            {
                visitor.VisitSession(new SessionMesg(e.mesg));
            }
            else if (e.mesg.Name == "Activity")
            {
                visitor.VisitActivity(new ActivityMesg(e.mesg));
            }
            else if (e.mesg.Name == "Record")
            {
                visitor.VisitRecord(new RecordMesg(e.mesg));
            }
        };

        using var fitStream = new FileStream(fitFilePath, FileMode.Open, FileAccess.Read);

        if (!decoder.IsFIT(fitStream))
        {
            throw new InvalidOperationException("Not a valid FIT file");
        }

        decoder.Read(fitStream);
        visitor.OnExtractionComplete();
    }
}