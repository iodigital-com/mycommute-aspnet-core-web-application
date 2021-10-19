using System.Collections.Generic;
using System.Security.Claims;

namespace MyCommute.Shared.Models.Authentication
{
    public record LoginResponse (string Token, List<Claim> Claims);
}