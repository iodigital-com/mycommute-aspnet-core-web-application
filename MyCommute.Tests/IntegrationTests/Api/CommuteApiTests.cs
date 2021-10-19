using System.Linq;
using AutoFixture;

namespace MyCommute.Tests.IntegrationTests.Api;

[TestFixture]
public class CommuteApiTests
{
    private readonly HttpClient client;
    private readonly Fixture fixture = new ();
    private Guid employeeId = Guid.Empty;
    private Guid commuteId = Guid.Empty;

    public CommuteApiTests()
    {
        client = new HttpClient()
        {
            BaseAddress = new Uri("https://localhost:5001")
        };
    }
    
    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        // Register a user
        var request = fixture.Build<UserRegistrationRequest>()
            .With(e => e.Email, $"{fixture.Create<string>()}@intracto.com")
            .With(x => x.HomeAddress, new Address("Meir", "1", "2000", "Antwerpen", "BE"))
            .With(x => x.WorkAddress, new Address("Grotesteenweg", "128", "2600", "Antwerpen"))
            .Create();

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var uri = $"{client.BaseAddress}User";
        var response = await client.PostAsync(uri, content);
        Assert.IsTrue(response.IsSuccessStatusCode);
        var result = await response.Content.ReadAsStringAsync();
        var userRegistrationResponse = JsonSerializer.Deserialize<UserRegistrationResponse>(result, new JsonSerializerOptions{ PropertyNameCaseInsensitive = true });
        
        Assert.IsNotNull(userRegistrationResponse);
        Assert.AreNotEqual(Guid.Empty, userRegistrationResponse!.Id);

        employeeId = userRegistrationResponse.Id;
        
        await AuthenticationHelper.LoginAsync(client, employeeId, request.Email);
    }
    
    [OneTimeTearDown]
    public async Task GlobalTearDown()
    {
        var uri = $"{client.BaseAddress}User?id={employeeId}";
        await client.DeleteAsync(uri);
    }

    [Test, Order(1)]
    public async Task ShouldAdd()
    {
        var request = new AddCommuteRequest(employeeId, CommuteType.Car, DateTime.Today);
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var uri = $"{client.BaseAddress}Commute";
        var response = await client.PostAsync(uri, content);
        
        Assert.IsTrue(response.IsSuccessStatusCode);
        
        var result = await response.Content.ReadAsStringAsync();
        var addCommuteResponse = JsonSerializer.Deserialize<AddOrUpdateCommuteResponse>(result, new JsonSerializerOptions{ PropertyNameCaseInsensitive = true });
        
        Assert.IsNotNull(addCommuteResponse);
        Assert.AreNotEqual(Guid.Empty, addCommuteResponse!.Id);

        commuteId = addCommuteResponse.Id;
    }
    
    [Test, Order(2)]
    public async Task ShouldGet()
    {
        var uri = $"{client.BaseAddress}Commute?employeeId={employeeId}";
        var response = await client.GetAsync(uri);
        
        Assert.IsTrue(response.IsSuccessStatusCode);
        
        var result = await response.Content.ReadAsStringAsync();
        
        IEnumerable<CommuteDto>? getCommutesResponse = JsonSerializer.Deserialize<IEnumerable<CommuteDto>>(result, new JsonSerializerOptions{ PropertyNameCaseInsensitive = true });
        
        Assert.IsNotNull(getCommutesResponse);
        Assert.IsTrue(getCommutesResponse!.Any());
        Assert.IsTrue(getCommutesResponse!.All(x => x.EmployeeId.Equals(employeeId)));
    }

    [Test, Order(3)]
    public async Task ShouldUpdate()
    {
        var request = new UpdateCommuteRequest(commuteId, CommuteType.Bike, DateTime.Today.AddDays(-3));
        
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var uri = $"{client.BaseAddress}Commute";
        var response = await client.PutAsync(uri, content);
        
        Assert.IsTrue(response.IsSuccessStatusCode);
        
        var result = await response.Content.ReadAsStringAsync();
        
        var updateCommuteResponse = JsonSerializer.Deserialize<AddOrUpdateCommuteResponse>(result, new JsonSerializerOptions{ PropertyNameCaseInsensitive = true });
        
        Assert.IsNotNull(updateCommuteResponse);
        Assert.AreEqual(request.Id, updateCommuteResponse!.Id);
        Assert.AreEqual(request.ModeOfTransport, updateCommuteResponse.ModeOfTransport);
        Assert.AreEqual(request.Date, updateCommuteResponse.Date);
    }

    [Order(4)]
    public async Task ShouldDelete()
    {
        var uri = $"{client.BaseAddress}Commute?id={commuteId}";
        var response = await client.DeleteAsync(uri);
        
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [Test]
    public async Task ShouldFail()
    {
        var request = new UpdateCommuteRequest(Guid.NewGuid(), CommuteType.Bike, DateTime.Today.AddDays(-3));
        
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var uri = $"{client.BaseAddress}Commute";
        var response = await client.PutAsync(uri, content);
        
        Assert.IsFalse(response.IsSuccessStatusCode);
        Assert.IsTrue(response.StatusCode.Equals(HttpStatusCode.NotFound));
    }
}