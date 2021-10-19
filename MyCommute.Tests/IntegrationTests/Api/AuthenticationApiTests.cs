using System.Linq;
using System.Security.Claims;
using AutoFixture;
using Microsoft.AspNetCore.Server.HttpSys;
using MyCommute.Shared.Models.Authentication;

namespace MyCommute.Tests.IntegrationTests.Api;

[TestFixture]
public class AuthenticationApiTests
{
    private readonly HttpClient client;
    private readonly Fixture fixture = new ();
    private Guid employeeId = Guid.Empty;
    private string email = string.Empty;

    public AuthenticationApiTests()
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
        email = request.Email;
    }
    
    [OneTimeTearDown]
    public async Task GlobalTearDown()
    {
        var uri = $"{client.BaseAddress}User?id={employeeId}";
        await client.DeleteAsync(uri);
    }

    [Test]
    public async Task LoginTest()
    {
        var request = new LoginRequest(employeeId, email);
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var uri = $"{client.BaseAddress}Authentication";
        
        var response = await client.PostAsync(uri, content);
        Assert.IsTrue(response.IsSuccessStatusCode); 
        Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        
        var result = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(result,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new ClaimJsonConverter() }});

        Assert.IsNotNull(loginResponse);
        Assert.IsFalse(string.IsNullOrWhiteSpace(loginResponse!.Token));
    }
}