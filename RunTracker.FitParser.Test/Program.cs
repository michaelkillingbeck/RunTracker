// Example: Extract Run object from FIT file
try
{
    var run = FitToRunConverter.ExtractRunFromFitFile("FITFiles\\sample.fit");
    
    if (run != null)
    {
        Console.WriteLine("Run Data Extracted:");
        Console.WriteLine($"ID: {run.Id}");
        Console.WriteLine($"Start Time: {run.StartTime:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Duration: {run.Duration:hh\\:mm\\:ss}");
        Console.WriteLine($"Distance: {run.DistanceKm:F2} km");
        Console.WriteLine($"Average Heart Rate: {run.AverageHeartRate} bpm");
        Console.WriteLine($"Average Pace: {run.AveragePace:mm\\:ss} per km");
    }
    else
    {
        Console.WriteLine("Could not extract run data from FIT file");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("\nPress any key to continue...");
Console.ReadKey();