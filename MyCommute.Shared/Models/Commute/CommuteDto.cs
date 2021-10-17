using System;

namespace MyCommute.Shared.Models.Commute
{
    public record CommuteDto(Guid Id, Guid EmployeeId, Enums.CommuteType ModeOfTransport, DateTime Date);
}