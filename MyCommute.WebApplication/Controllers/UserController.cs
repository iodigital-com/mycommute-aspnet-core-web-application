namespace MyCommute.WebApplication.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> logger;
    private readonly IEmployeeService employeeService;
    private readonly IGeoCodeService geoCodeService;

    public UserController(ILogger<UserController> logger, IEmployeeService employeeService, IGeoCodeService geoCodeService)
    {
        this.logger = logger;
        this.employeeService = employeeService;
        this.geoCodeService = geoCodeService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserRegistrationResponse>> Register(UserRegistrationRequest request)
    {
        try
        {
            var homeLocation = await geoCodeService.GetCoordinatesForAddressAsync(request.HomeAddress);
            var workLocation = await geoCodeService.GetCoordinatesForAddressAsync(request.WorkAddress);

            Employee employee = await employeeService.AddAsync(new()
            {
                Name = request.Name,
                Email = request.Email,
                DefaultCommuteType = request.DefaultCommuteType,
                HomeLocation = homeLocation,
                DefaultWorkLocation = workLocation
            });

            return new UserRegistrationResponse(employee.Id);
        }
        catch (ForwardGeoCodeFailedException geoCodeFailedException)
        {
            return BadRequest(geoCodeFailedException.Message);
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
    public async Task<ActionResult> Update(UserUpdateRequest request)
    {
        try
        {
            var homeLocation = await geoCodeService.GetCoordinatesForAddressAsync(request.HomeAddress);
            var workLocation = await geoCodeService.GetCoordinatesForAddressAsync(request.WorkAddress);
            
            Employee employee = await employeeService.UpdateAsync(new()
            {
                Id = request.Id,
                Name = request.Name,
                DefaultCommuteType = request.DefaultCommuteType,
                HomeLocation = homeLocation,
                DefaultWorkLocation = workLocation
            });

            return Ok();
        }
        catch (ForwardGeoCodeFailedException geoCodeFailedException)
        {
            return BadRequest(geoCodeFailedException.Message);
        }
        catch (EmployeeNotFoundException)
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
            await employeeService.DeleteAsync(id);

            return Ok();
        }
        catch (EmployeeNotFoundException)
        {
            return NotFound();
        }
    }
}