using System;

namespace MyCommute.Shared.Models.Commute
{
    public record AddCommuteRequest(Guid EmployeeId, Enums.CommuteType ModeOfTransport, DateTime Date);
}