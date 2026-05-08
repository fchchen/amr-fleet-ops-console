using AmrFleet.Api.Models;
using AmrFleet.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AmrFleet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RobotsController(IRobotFleetStore store, RobotCommandService commands) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<RobotTelemetryDto>> GetRobots()
    {
        return Ok(store.GetRobots().Select(RobotTelemetryDto.FromRobot));
    }

    [HttpGet("{id}")]
    public ActionResult<RobotTelemetryDto> GetRobot(string id)
    {
        var robot = store.GetRobot(id);
        return robot is null ? NotFound() : Ok(RobotTelemetryDto.FromRobot(robot));
    }

    [HttpPost("{id}/emergency-stop")]
    public IActionResult EmergencyStop(string id)
    {
        return commands.EmergencyStop(id) ? NoContent() : NotFound();
    }

    [HttpPost("{id}/enter-recovery-mode")]
    public IActionResult EnterRecoveryMode(string id)
    {
        var result = commands.EnterRecoveryMode(id);
        return ToActionResult(result);
    }

    [HttpPost("{id}/exit-recovery-mode")]
    public IActionResult ExitRecoveryMode(string id)
    {
        return commands.ExitRecoveryMode(id) ? NoContent() : NotFound();
    }

    [HttpPost("{id}/teleop-command")]
    public IActionResult TeleopCommand(string id, TeleopCommandRequest request)
    {
        var result = commands.Teleop(id, request.Command);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult(CommandResult result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        if (result.IsNotFound)
        {
            return NotFound(new { message = result.Error });
        }

        return Conflict(new { message = result.Error });
    }
}
