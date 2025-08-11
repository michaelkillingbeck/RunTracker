using RunTracker.FitParser;
using RunTracker.Models;

namespace RunTracker.FitParser.Tests;

public class FitToRunConverterTests
{
    private const string SampleFitFile = "TestFiles/sample.fit";
    private const string NonExistentFile = "TestFiles/nonexistent.fit";

    [Fact]
    public void ExtractRunFromFitFile_WithValidFile_ReturnsRun()
    {
        // Act
        var result = FitToRunConverter.ExtractRunFromFitFile(SampleFitFile);

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
        var result = FitToRunConverter.ExtractRunFromFitFile(SampleFitFile);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(9.31f, result.DistanceKm, precision: 2);
        Assert.Equal(133, result.AverageHeartRate);
        Assert.Equal(3459.218, result.Duration.TotalSeconds, precision: 3);
        Assert.Equal(new DateTime(2025, 8, 7, 18, 16, 55), result.StartTime);
        Assert.True(result.AveragePace > TimeSpan.Zero);
    }

    [Fact]
    public void ExtractRunFromFitFile_CalculatesAveragePaceCorrectly()
    {
        // Act
        var result = FitToRunConverter.ExtractRunFromFitFile(SampleFitFile);

        // Assert
        Assert.NotNull(result);
        
        // Calculate expected pace: duration / distance
        var expectedPaceSeconds = result.Duration.TotalSeconds / result.DistanceKm;
        
        Assert.Equal(expectedPaceSeconds, result.AveragePace.TotalSeconds, precision: 6);
    }

    [Fact]
    public void ExtractRunFromFitFile_WithNonExistentFile_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            FitToRunConverter.ExtractRunFromFitFile(NonExistentFile));
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
        var result1 = FitToRunConverter.ExtractRunFromFitFile(SampleFitFile);
        var result2 = FitToRunConverter.ExtractRunFromFitFile(SampleFitFile);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotEqual(result1.Id, result2.Id);
    }

    [Fact]
    public void ExtractRunFromFitFile_AllPropertiesAreSet()
    {
        // Act
        var result = FitToRunConverter.ExtractRunFromFitFile(SampleFitFile);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(DateTime.MinValue, result.StartTime);
        Assert.NotEqual(TimeSpan.Zero, result.Duration);
        Assert.True(result.DistanceKm > 0);
        Assert.True(result.AverageHeartRate >= 0);
        Assert.True(result.AveragePace >= TimeSpan.Zero);
    }
}