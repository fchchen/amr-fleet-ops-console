namespace AmrFleet.Api.Options;

public class FleetConsoleOptions
{
    public const string SectionName = "FleetConsole";

    public string[] CorsOrigins { get; set; } = ["http://localhost:4200", "http://localhost:8080"];
    public int SimulatorTickMilliseconds { get; set; } = 1000;
    public double MapMinimumCoordinate { get; set; } = 4;
    public double MapMaximumCoordinate { get; set; } = 96;
}
