using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace MyCommute.WebApplication.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> logger;
    private readonly IOptions<JwtConfig> jwtConfiguration;
    private readonly IEmployeeService employeeService;
    
    public AuthenticationController(ILogger<AuthenticationController> logger, IOptions<JwtConfig> jwtConfiguration, IEmployeeService employeeService)
    {
        this.logger = logger;
        this.jwtConfiguration = jwtConfiguration;
        this.employeeService = employeeService;
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        try
        {
            var employee = await employeeService.GetByIdAsync(request.Id);

            if (employee.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
            {
                return GenerateJwtToken(employee.Id, employee.Email);
            }
        }
        catch (EmployeeNotFoundException)
        {
            return Unauthorized();
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception.Message);
        }

        return Unauthorized();
    }
    
    private LoginResponse GenerateJwtToken(Guid id, string email)
    {
        // generate token that is valid for 3 months
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII
            .GetBytes(jwtConfiguration.Value.Secret);
            
        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, email),
            new (ClaimTypes.Sid, id.ToString())
        };
            
        var claimsIdentity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
            
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.AddMonths(3),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
            
        var token = tokenHandler.CreateToken(tokenDescriptor);
            
        return new LoginResponse(tokenHandler.WriteToken(token), claims);
    }
}