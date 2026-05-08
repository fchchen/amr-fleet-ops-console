using AmrFleet.Api.Models;
using AmrFleet.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AmrFleet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MissionsController(IRobotFleetStore store, MissionService missions) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Mission>> GetMissions()
    {
        return Ok(store.GetMissions());
    }

    [HttpPost]
    public ActionResult<Mission> CreateMission(MissionCreateRequest request)
    {
        try
        {
            var mission = missions.CreateMission(request);
            return CreatedAtAction(nameof(GetMissions), new { id = mission.Id }, mission);
        }
        catch (MissionCreateException ex) when (ex.Failure == MissionCreateFailure.RobotNotFound)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (MissionCreateException ex) when (ex.Failure == MissionCreateFailure.InvalidRequest)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (MissionCreateException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    public IActionResult CancelMission(string id)
    {
        return missions.CancelMission(id) ? NoContent() : NotFound();
    }
}
