namespace MyCommute.Tests.IntegrationTests.Services;

[TestFixture]
public class GeoCodeServiceTests
{
    private readonly IGeoCodeService geoCodeService;

    public GeoCodeServiceTests()
    {
        geoCodeService = new GeoCodeService();
    }

    [Test]
    public async Task GetCoordinatesForAddressTest()
    {
        Address address = new ((string)"Grotesteenweg",(string)"214", (string)"2600",(string)"Antwerpen", (string?)"BE");

        Point coordinates = await geoCodeService.GetCoordinatesForAddressAsync(address);
        
        Assert.AreEqual(4.4224318731099341, coordinates.X);
        Assert.AreEqual(51.193298900000002, coordinates.Y);
    }
    
    [Test]
    public async Task GetAddressForCoordinatesTest()
    {
        var coordinates = new Point(4.4224318731099341, 51.193298900000002);
        
        var address = await geoCodeService.GetAddressForCoordinatesAsync(coordinates);
        
        Assert.AreEqual("Grotesteenweg", address.Street);
        Assert.AreEqual("214", address.Number);
        Assert.AreEqual("2600", address.ZipCode);
        //Assert.AreEqual("Antwerpen", address.City);
        Assert.AreEqual("be", address.CountryIso2Code);
    }
}