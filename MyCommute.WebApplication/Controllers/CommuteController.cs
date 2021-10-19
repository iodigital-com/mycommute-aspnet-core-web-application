using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using MyCommute.Domain.Exceptions;
using MyCommute.Shared.Models.Commute;

namespace MyCommute.WebApplication.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class CommuteController : ControllerBase
{
    private readonly ILogger<CommuteController> logger;
    private readonly ICommuteService commuteService;

    public CommuteController(ILogger<CommuteController> logger, ICommuteService commuteService)
    {
        this.logger = logger;
        this.commuteService = commuteService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CommuteDto>>> Get(Guid employeeId)
    {
        try
        {
            var commutes = await commuteService.GetByUserIdAsync(employeeId);
            if (commutes.Any())
            {
                return commutes.Select(x => new CommuteDto(x.Id, x.EmployeeId, x.Type, x.Date))
                    .ToList();
            }
        }
        catch (CommuteNotFoundException)
        {
            return NotFound();
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception.Message);
        }

        return BadRequest();
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AddOrUpdateCommuteResponse>> Add(AddCommuteRequest request)
    {
        try
        {
            Commute commute = await commuteService.AddAsync(new()
            {
                EmployeeId = request.EmployeeId,
                Type = request.ModeOfTransport,
                Date = request.Date,
            });

            return new JsonResult(
                new AddOrUpdateCommuteResponse(commute.Id, commute.Type, commute.Date)
                );
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception.Message);
        }

        return BadRequest();
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddOrUpdateCommuteResponse>> Update(UpdateCommuteRequest request)
    {
        try
        {
            Commute commute = await commuteService.UpdateAsync(new()
            {
                Id = request.Id,
                Type = request.ModeOfTransport,
                Date = request.Date,
            });

            return new JsonResult(
                new AddOrUpdateCommuteResponse(commute.Id, commute.Type, commute.Date)
            );
        }
        catch (CommuteNotFoundException)
        {
            return NotFound();
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception.Message);
        }

        return BadRequest();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await commuteService.DeleteAsync(id);

            return Ok();
        }
        catch (CommuteNotFoundException)
        {
            return NotFound();
        }
    }
}