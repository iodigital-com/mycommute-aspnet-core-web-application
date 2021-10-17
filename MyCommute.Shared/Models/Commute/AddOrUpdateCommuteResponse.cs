using System;

namespace MyCommute.Shared.Models.Commute
{
    public record AddOrUpdateCommuteResponse(Guid Id, Enums.CommuteType ModeOfTransport, DateTime Date);
}