namespace MyCommute.Shared.Models.User
{
    public record UserRegistrationRequest (string Name, string Email, Address HomeAddress, Address WorkAddress, Enums.CommuteType DefaultCommuteType);
}