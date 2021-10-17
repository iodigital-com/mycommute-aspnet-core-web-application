
using NetTopologySuite.Geometries;
namespace MyCommute.Domain.Services;

public interface IGeoCodeService
{
    Task<Point> GetCoordinatesForAddressAsync(Address address);
    Task<Address> GetAddressForCoordinatesAsync(Point coordinates);
}