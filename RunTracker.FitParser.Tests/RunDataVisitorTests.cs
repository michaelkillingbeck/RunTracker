using Dynastream.Fit;

namespace RunTracker.FitParser.Tests;

public class RunDataVisitorTests
{
    [Fact]
    public void Constructor_InitializesPropertiesToDefaultValues()
    {
        // Act
        var visitor = new RunDataVisitor();

        // Assert
        Assert.Null(visitor.SessionMessage);
        Assert.Null(visitor.ActivityMessage);
        Assert.Equal(0, visitor.AverageHeartRate);
        Assert.Equal(0, visitor.FinalDistance);
    }

    [Fact]
    public void VisitSession_StoresSessionMessage()
    {
        // Arrange
        var visitor = new RunDataVisitor();
        var sessionMessage = new SessionMesg();

        // Act
        visitor.VisitSession(sessionMessage);

        // Assert
        Assert.Equal(sessionMessage, visitor.SessionMessage);
    }

    [Fact]
    public void VisitActivity_StoresActivityMessage()
    {
        // Arrange
        var visitor = new RunDataVisitor();
        var activityMessage = new ActivityMesg();

        // Act
        visitor.VisitActivity(activityMessage);

        // Assert
        Assert.Equal(activityMessage, visitor.ActivityMessage);
    }

    [Fact]
    public void VisitSession_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var visitor = new RunDataVisitor();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => visitor.VisitSession(null!));
    }

    [Fact]
    public void VisitActivity_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var visitor = new RunDataVisitor();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => visitor.VisitActivity(null!));
    }

    [Fact]
    public void VisitRecord_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var visitor = new RunDataVisitor();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => visitor.VisitRecord(null!));
    }

    [Fact]
    public void VisitRecord_WithHeartRateData_CalculatesAverageCorrectly()
    {
        // Arrange
        var visitor = new RunDataVisitor();
        var record1 = CreateRecordWithHeartRate(120);
        var record2 = CreateRecordWithHeartRate(140);
        var record3 = CreateRecordWithHeartRate(130);

        // Act
        visitor.VisitRecord(record1);
        visitor.VisitRecord(record2);
        visitor.VisitRecord(record3);

        // Assert
        Assert.Equal(130f, visitor.AverageHeartRate); // (120 + 140 + 130) / 3 = 130
    }

    [Fact]
    public void VisitRecord_WithMixedHeartRateData_IgnoresNullValues()
    {
        // Arrange
        var visitor = new RunDataVisitor();
        var recordWithHR = CreateRecordWithHeartRate(120);
        var recordWithoutHR = CreateRecordWithHeartRate(null);
        var recordWithHR2 = CreateRecordWithHeartRate(140);

        // Act
        visitor.VisitRecord(recordWithHR);
        visitor.VisitRecord(recordWithoutHR);
        visitor.VisitRecord(recordWithHR2);

        // Assert
        Assert.Equal(130f, visitor.AverageHeartRate); // (120 + 140) / 2 = 130
    }

    [Fact]
    public void VisitRecord_WithNoHeartRateData_ReturnsZero()
    {
        // Arrange
        var visitor = new RunDataVisitor();
        var recordWithoutHR = CreateRecordWithHeartRate(null);

        // Act
        visitor.VisitRecord(recordWithoutHR);

        // Assert
        Assert.Equal(0f, visitor.AverageHeartRate);
    }

    [Fact]
    public void VisitRecord_WithDistanceData_TracksLatestDistance()
    {
        // Arrange
        var visitor = new RunDataVisitor();
        var earlierTime = new Dynastream.Fit.DateTime(1000);
        var laterTime = new Dynastream.Fit.DateTime(2000);
        
        var record1 = CreateRecordWithDistanceAndTime(1500f, earlierTime);
        var record2 = CreateRecordWithDistanceAndTime(3600f, laterTime);

        // Act
        visitor.VisitRecord(record1);
        visitor.VisitRecord(record2);

        // Assert
        Assert.Equal(3600f, visitor.FinalDistance);
    }

    [Fact]
    public void VisitRecord_WithOutOfOrderTimestamps_KeepsLatestByTime()
    {
        // Arrange
        var visitor = new RunDataVisitor();
        var earlierTime = new Dynastream.Fit.DateTime(1000);
        var laterTime = new Dynastream.Fit.DateTime(2000);
        
        var laterRecord = CreateRecordWithDistanceAndTime(3600f, laterTime);
        var earlierRecord = CreateRecordWithDistanceAndTime(1500f, earlierTime);

        // Act - Process later record first, then earlier
        visitor.VisitRecord(laterRecord);
        visitor.VisitRecord(earlierRecord);

        // Assert - Should keep the later record's distance
        Assert.Equal(3600f, visitor.FinalDistance);
    }

    [Fact]
    public void VisitRecord_WithNoTimestamp_IgnoresDistance()
    {
        // Arrange
        var visitor = new RunDataVisitor();
        var recordWithoutTime = CreateRecordWithDistanceAndTime(1500f, null);

        // Act
        visitor.VisitRecord(recordWithoutTime);

        // Assert
        Assert.Equal(0f, visitor.FinalDistance);
    }

    private static RecordMesg CreateRecordWithHeartRate(byte? heartRate)
    {
        var record = new RecordMesg();
        if (heartRate.HasValue)
        {
            record.SetHeartRate(heartRate.Value);
        }
        return record;
    }

    private static RecordMesg CreateRecordWithDistanceAndTime(float? distance, Dynastream.Fit.DateTime? timestamp)
    {
        var record = new RecordMesg();
        if (distance.HasValue)
        {
            record.SetDistance(distance.Value);
        }
        if (timestamp != null)
        {
            record.SetTimestamp(timestamp);
        }
        return record;
    }
}