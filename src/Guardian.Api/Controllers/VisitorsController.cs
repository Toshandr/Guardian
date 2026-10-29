using Guardian.Api.DTOs;
using Guardian.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Guardian.Api.Controllers;


[ApiController]
[Route("api/visitors")]
public class VisitorsController(IVisitorService visitors) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateVisitorResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateVisitorResponse>> Create()
    {
        var visitor = await visitors.CreateAsync();
        return CreatedAtAction(nameof(GetById), new { id = visitor.Id }, visitor);
    }

    [HttpGet]
    [ProducesResponseType<List<VisitorResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VisitorResponse>>> GetAll() =>
        Ok(await visitors.GetAllAsync());

    [HttpGet("{id:int}")]
    [ProducesResponseType<VisitorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VisitorResponse>> GetById(int id)
    {
        var visitor = await visitors.GetByIdAsync(id);
        return visitor is null ? NotFound() : Ok(visitor);
    }

    [HttpPost("{id:int}/actions")]
    [ProducesResponseType<VisitorActionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VisitorActionResponse>> PerformAction(int id, VisitorActionRequest request)
    {
        try
        {
            var result = await visitors.PerformActionAsync(id, request.Action);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("{id:int}/access")]
    [ProducesResponseType<AccessResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccessResponse>> CheckAccess(int id)
    {
        var result = await visitors.CheckAccessAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}
