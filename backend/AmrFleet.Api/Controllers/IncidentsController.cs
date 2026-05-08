using AmrFleet.Api.Models;
using AmrFleet.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AmrFleet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentsController(IRobotFleetStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Incident>> GetIncidents([FromQuery] IncidentStatus? status = null)
    {
        return Ok(store.GetIncidents(status));
    }

    [HttpGet("{id}")]
    public ActionResult<Incident> GetIncident(string id)
    {
        var incident = store.GetIncident(id);
        return incident is null ? NotFound() : Ok(incident);
    }

    [HttpPost("{id}/acknowledge")]
    public IActionResult AcknowledgeIncident(string id)
    {
        return store.AcknowledgeIncident(id) ? NoContent() : NotFound();
    }

    [HttpPost("{id}/resolve")]
    public IActionResult ResolveIncident(string id)
    {
        return store.ResolveIncident(id) ? NoContent() : NotFound();
    }
}
