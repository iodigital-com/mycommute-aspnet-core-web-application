using System;

namespace MyCommute.Shared.Models.User
{
    public record UserUpdateRequest(Guid Id, string Name, Address HomeAddress, Address WorkAddress, Enums.CommuteType DefaultCommuteType);
}