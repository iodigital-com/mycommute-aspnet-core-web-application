using AutoFixture;

namespace MyCommute.Tests.IntegrationTests.Api;

[TestFixture]
public class UserApiTests
{
    private readonly HttpClient client;
    private readonly Fixture fixture = new ();
    private Guid employeeId = Guid.Empty;

    public UserApiTests()
    {
        client = new HttpClient()
        {
            BaseAddress = new Uri("https://localhost:5001")
        };
    }
    
    [Test, Order(1)]
    public async Task ShouldRegister()
    {
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

    [Test]
    public async Task ShouldFail()
    {
        // Register with incorrect street name, produces BadRequest
        var request = fixture.Build<UserRegistrationRequest>()
            .With(e => e.Email, $"{fixture.Create<string>()}@intracto.com")
            .With(x => x.HomeAddress, new Address("Mier", "1", "2000", "Antwerpen", "BE"))
            .With(x => x.WorkAddress, new Address("Grotesteenweg", "128", "2600", "Antwerpen"))
            .Create();

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var uri = $"{client.BaseAddress}User";
        var response = await client.PostAsync(uri, content);
        
        Assert.IsFalse(response.IsSuccessStatusCode);
        Assert.IsTrue(response.StatusCode.Equals(HttpStatusCode.BadRequest));
    }
    
    [Test, Order(2)]
    public async Task ShouldUpdate()
    {
        var request = fixture.Build<UserUpdateRequest>()
            .With(x => x.Id, employeeId)
            .With(x => x.HomeAddress, new Address("Meir", "1", "2000", "Antwerpen", "BE"))
            .With(x => x.WorkAddress, new Address("Chaussée de La Hulpe", "120", "1000", "Bruxelles"))
            .With(x => x.DefaultCommuteType, CommuteType.PublicTransport)
            .Create();

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var uri = $"{client.BaseAddress}User";
        var response = await client.PutAsync(uri, content);
        
        Assert.IsTrue(response.IsSuccessStatusCode);
    }
    
    [Test, Order(3)]
    public async Task ShouldDelete()
    {
        var uri = $"{client.BaseAddress}User?id={employeeId}";
        var response = await client.DeleteAsync(uri);
        
        Assert.IsTrue(response.IsSuccessStatusCode);
    }
}