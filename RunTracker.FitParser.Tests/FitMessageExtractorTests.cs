using Dynastream.Fit;
using RunTracker.FitParser.Interfaces;
using File = System.IO.File;

namespace RunTracker.FitParser.Tests;

public class FitMessageExtractorTests
{
    private const string sampleFitFile = "TestFiles/sample.fit";
    private const string nonExistentFile = "TestFiles/nonexistent.fit";

    [Fact]
    public void ExtractMessages_WithNullVisitor_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            FitMessageExtractor.ExtractMessages(sampleFitFile, null!));
    }

    [Fact]
    public void ExtractMessages_WithValidFile_CallsVisitorMethods()
    {
        // Arrange
        var visitor = new MockVisitor();

        // Act
        FitMessageExtractor.ExtractMessages(sampleFitFile, visitor);

        // Assert
        Assert.True(visitor.SessionVisited);
        Assert.True(visitor.ActivityVisited);
        Assert.True(visitor.RecordsVisited > 0);
        Assert.True(visitor.ExtractionCompleted);
    }

    [Fact]
    public void ExtractMessages_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var visitor = new MockVisitor();

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            FitMessageExtractor.ExtractMessages(nonExistentFile, visitor));
    }

    [Fact]
    public void ExtractMessages_WithInvalidFile_ThrowsInvalidOperationException()
    {
        // Arrange
        var visitor = new MockVisitor();
        var invalidFile = "TestFiles/invalid.txt";
        File.WriteAllText(invalidFile, "This is not a FIT file");

        try
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                FitMessageExtractor.ExtractMessages(invalidFile, visitor));

            Assert.Equal("Not a valid FIT file", exception.Message);
        }
        finally
        {
            // Cleanup
            if (File.Exists(invalidFile))
                File.Delete(invalidFile);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ExtractMessages_WithInvalidPath_ThrowsException(string? filePath)
    {
        // Arrange
        var visitor = new MockVisitor();

        // Act & Assert
        Assert.ThrowsAny<Exception>(() =>
            FitMessageExtractor.ExtractMessages(filePath!, visitor));
    }

    private class MockVisitor : IFitMessageVisitor
    {
        public bool SessionVisited { get; private set; }
        public bool ActivityVisited { get; private set; }
        public int RecordsVisited { get; private set; }
        public bool ExtractionCompleted { get; private set; }

        public void VisitRecord(RecordMesg record)
        {
            ArgumentNullException.ThrowIfNull(record);
            RecordsVisited++;
        }

        public void VisitSession(SessionMesg session)
        {
            ArgumentNullException.ThrowIfNull(session);
            SessionVisited = true;
        }

        public void VisitActivity(ActivityMesg activity)
        {
            ArgumentNullException.ThrowIfNull(activity);
            ActivityVisited = true;
        }

        public void OnExtractionComplete()
        {
            ExtractionCompleted = true;
        }
    }
}