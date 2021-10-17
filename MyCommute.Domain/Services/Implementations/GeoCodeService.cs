using NetTopologySuite;
using Nominatim.API.Geocoders;
using Nominatim.API.Models;
using NetTopologySuite.Geometries;

namespace MyCommute.Domain.Services.Implementations;

public class GeoCodeService : IGeoCodeService
{
    public async Task<Point> GetCoordinatesForAddressAsync(Address address)
    {
        var geocoder = new ForwardGeocoder();

        var request = new ForwardGeocodeRequest
        {
            queryString = address.ToQueryString(),

            BreakdownAddressElements = true,
            ShowExtraTags = true,
            ShowAlternativeNames = true,
            ShowGeoJSON = true
        };

        GeocodeResponse[] result = await geocoder.Geocode(request);

        if (result.Length == 0)
        {
            throw new ForwardGeoCodeFailedException(address.ToQueryString());
        }
        
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        return geometryFactory.CreatePoint(new Coordinate(result[0].Longitude, result[0].Latitude));
    }

    public async Task<Address> GetAddressForCoordinatesAsync(Point coordinates)
    {
        var geocoder = new ReverseGeocoder();

        var request = new ReverseGeocodeRequest
        {
            Longitude = coordinates.X,
            Latitude = coordinates.Y,

            BreakdownAddressElements = true,
            ShowExtraTags = true,
            ShowAlternativeNames = true,
            ShowGeoJSON = true
        };

        GeocodeResponse? result = await geocoder.ReverseGeocode(request);

        if (result.PlaceID == 0)
        {
            throw new Exception();
        }

        return new Address(result.Address.Road, result.Address.HouseNumber, result.Address.PostCode,
            result.Address.City, result.Address.CountryCode);

    }
}