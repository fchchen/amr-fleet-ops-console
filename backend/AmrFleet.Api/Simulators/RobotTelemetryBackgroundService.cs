using AmrFleet.Api.Hubs;
using AmrFleet.Api.Models;
using AmrFleet.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace AmrFleet.Api.Simulators;

public class RobotTelemetryBackgroundService(
    RobotMovementSimulator simulator,
    IRobotFleetStore store,
    IHubContext<RobotTelemetryHub> hubContext,
    ILogger<RobotTelemetryBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                simulator.Tick();
                var telemetry = store.GetRobots().Select(RobotTelemetryDto.FromRobot).ToList();
                await hubContext.Clients.All.SendAsync("robotTelemetryUpdated", telemetry, stoppingToken);
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Telemetry simulator tick failed.");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
