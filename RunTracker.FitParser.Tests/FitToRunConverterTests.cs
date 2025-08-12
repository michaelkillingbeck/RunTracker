using Dynastream.Fit;
using DateTime = System.DateTime;
using File = System.IO.File;

namespace RunTracker.FitParser.Tests;

public class FitToRunConverterTests
{
    private const string sampleFitFile = "TestFiles/sample.fit";
    private const string nonExistentFile = "TestFiles/nonexistent.fit";

    [Fact]
    public void ExtractRunFromFitFile_WithValidFile_ReturnsRun()
    {
        // Act
        var result = FitToRunConverter.ExtractRunFromFitFile(sampleFitFile);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(result.DistanceKm > 0);
        Assert.True(result.Duration > TimeSpan.Zero);
        Assert.True(result.StartTime > DateTime.MinValue);
    }

    [Fact]
    public void ExtractRunFromFitFile_WithValidFile_ReturnsExpectedValues()
    {
        // Act
        var result = FitToRunConverter.ExtractRunFromFitFile(sampleFitFile);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3.60f, result.DistanceKm, precision: 2);
        Assert.Equal(126, result.AverageHeartRate);
        Assert.Equal(3601, result.Duration.TotalSeconds, precision: 3);
        Assert.Equal(new DateTime(2021, 07, 20, 22, 11, 21), result.StartTime);
        Assert.True(result.AveragePace > TimeSpan.Zero);
    }

    [Fact]
    public void ExtractRunFromFitFile_CalculatesAveragePaceCorrectly()
    {
        // Act
        var result = FitToRunConverter.ExtractRunFromFitFile(sampleFitFile);

        // Assert
        Assert.NotNull(result);
        
        // Calculate expected pace: duration / distance
        var expectedPaceSecondsPerKm = result.Duration.TotalSeconds / result.DistanceKm;
        
        Assert.Equal(expectedPaceSecondsPerKm, result.AveragePace.TotalSeconds, precision: 1);
    }

    [Fact]
    public void ExtractRunFromFitFile_WithNonExistentFile_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            FitToRunConverter.ExtractRunFromFitFile(nonExistentFile));
    }

    [Fact]
    public void ExtractRunFromFitFile_WithInvalidFile_ThrowsInvalidOperationException()
    {
        // Arrange - Create a dummy non-FIT file
        var invalidFile = "TestFiles/invalid.txt";
        File.WriteAllText(invalidFile, "This is not a FIT file");

        try
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                FitToRunConverter.ExtractRunFromFitFile(invalidFile));
            
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
    public void ExtractRunFromFitFile_WithInvalidPath_ThrowsException(string? filePath)
    {
        // Act & Assert
        Assert.ThrowsAny<Exception>(() =>
            FitToRunConverter.ExtractRunFromFitFile(filePath!));
    }

    [Fact]
    public void ExtractRunFromFitFile_GeneratesUniqueIds()
    {
        // Act
        var result1 = FitToRunConverter.ExtractRunFromFitFile(sampleFitFile);
        var result2 = FitToRunConverter.ExtractRunFromFitFile(sampleFitFile);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotEqual(result1.Id, result2.Id);
    }

    [Fact]
    public void ExtractRunFromFitFile_AllPropertiesAreSet()
    {
        // Act
        var result = FitToRunConverter.ExtractRunFromFitFile(sampleFitFile);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(DateTime.MinValue, result.StartTime);
        Assert.NotEqual(TimeSpan.Zero, result.Duration);
        Assert.True(result.DistanceKm > 0);
        Assert.True(result.AverageHeartRate >= 0);
        Assert.True(result.AveragePace >= TimeSpan.Zero);
    }

    // Unit tests for Create method
    [Fact]
    public void Create_WithNullVisitor_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FitToRunConverter.Create(null!));
    }

    [Fact]
    public void Create_WithValidVisitor_ReturnsRun()
    {
        // Arrange
        var visitor = CreateMockVisitor(
            distanceMeters: 3600f,
            averageHeartRate: 150f,
            durationSeconds: 1800f,
            startTime: new DateTime(2021, 7, 20, 22, 11, 21)
        );

        // Act
        var result = FitToRunConverter.Create(visitor);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(3.6f, result.DistanceKm, precision: 2);
        Assert.Equal(150, result.AverageHeartRate);
        Assert.Equal(1800, result.Duration.TotalSeconds);
        Assert.Equal(new DateTime(2021, 7, 20, 22, 11, 21), result.StartTime);
    }

    [Fact]
    public void Create_CalculatesAveragePaceCorrectly()
    {
        // Arrange
        var visitor = CreateMockVisitor(
            distanceMeters: 5000f, // 5km
            averageHeartRate: 140f,
            durationSeconds: 1500f, // 25 minutes
            startTime: DateTime.UtcNow
        );

        // Act
        var result = FitToRunConverter.Create(visitor);

        // Assert
        var expectedPaceSecondsPerKm = 1500f / 5f; // 300 seconds per km (5 minutes)
        Assert.Equal(expectedPaceSecondsPerKm, result.AveragePace.TotalSeconds, precision: 1);
    }

    [Fact]
    public void Create_WithZeroDistance_ReturnsZeroPace()
    {
        // Arrange
        var visitor = CreateMockVisitor(
            distanceMeters: 0f,
            averageHeartRate: 0f,
            durationSeconds: 1800f,
            startTime: DateTime.UtcNow
        );

        // Act
        var result = FitToRunConverter.Create(visitor);

        // Assert
        Assert.Equal(0f, result.DistanceKm);
        Assert.Equal(TimeSpan.Zero, result.AveragePace);
    }

    [Fact]
    public void Create_FallsBackToSessionDistance_WhenNoRecordDistance()
    {
        // Arrange
        var visitor = CreateMockVisitor(
            distanceMeters: 0f, // No record distance
            averageHeartRate: 130f,
            durationSeconds: 1200f,
            startTime: DateTime.UtcNow,
            sessionDistanceMeters: 2500f // Session has distance
        );

        // Act
        var result = FitToRunConverter.Create(visitor);

        // Assert
        Assert.Equal(2.5f, result.DistanceKm, precision: 2);
    }

    [Fact]
    public void Create_FallsBackToSessionHeartRate_WhenNoRecordHeartRate()
    {
        // Arrange
        var visitor = CreateMockVisitor(
            distanceMeters: 1000f,
            averageHeartRate: 0f, // No record heart rate
            durationSeconds: 600f,
            startTime: DateTime.UtcNow,
            sessionHeartRate: 145
        );

        // Act
        var result = FitToRunConverter.Create(visitor);

        // Assert
        Assert.Equal(145, result.AverageHeartRate);
    }

    private static RunDataVisitor CreateMockVisitor(
        float distanceMeters,
        float averageHeartRate,
        float durationSeconds,
        DateTime startTime,
        float? sessionDistanceMeters = null,
        byte? sessionHeartRate = null)
    {
        var visitor = new RunDataVisitor();

        // Create session message
        var sessionMessage = new SessionMesg();
        sessionMessage.SetTotalTimerTime(durationSeconds);
        if (sessionDistanceMeters.HasValue)
        {
            sessionMessage.SetTotalDistance(sessionDistanceMeters.Value);
        }
        if (sessionHeartRate.HasValue)
        {
            sessionMessage.SetAvgHeartRate(sessionHeartRate.Value);
        }

        // Create activity message  
        var activityMessage = new ActivityMesg();
        activityMessage.SetTimestamp(new Dynastream.Fit.DateTime(startTime));

        // Populate visitor
        visitor.VisitSession(sessionMessage);
        visitor.VisitActivity(activityMessage);

        // Add record message if distance/heart rate specified
        if (distanceMeters > 0 || averageHeartRate > 0)
        {
            var recordMessage = new RecordMesg();
            recordMessage.SetTimestamp(new Dynastream.Fit.DateTime(startTime.AddSeconds(durationSeconds)));
            
            if (distanceMeters > 0)
            {
                recordMessage.SetDistance(distanceMeters);
            }
            
            if (averageHeartRate > 0)
            {
                // Simulate multiple heart rate readings that average to the target
                for (int i = 0; i < 10; i++)
                {
                    var hrRecord = new RecordMesg();
                    hrRecord.SetHeartRate((byte)averageHeartRate);
                    visitor.VisitRecord(hrRecord);
                }
            }
            
            visitor.VisitRecord(recordMessage);
        }

        visitor.OnExtractionComplete();
        return visitor;
    }
}