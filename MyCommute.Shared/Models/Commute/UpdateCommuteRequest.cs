using System;

namespace MyCommute.Shared.Models.Commute
{
    public record UpdateCommuteRequest(Guid Id, Enums.CommuteType ModeOfTransport, DateTime Date);
}