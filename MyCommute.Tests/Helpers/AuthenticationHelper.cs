namespace MyCommute.Tests.Helpers;

public class AuthenticationHelper
{
    public static async Task LoginAsync(HttpClient client, Guid employeeId, string email)
    {
        var loginRequest = new LoginRequest(employeeId, email);
        var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var uri = $"{client.BaseAddress}Authentication";

        var response = await client.PostAsync(uri, content);
        Assert.IsTrue(response.IsSuccessStatusCode);
        Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(result,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new ClaimJsonConverter() }});

        Assert.IsNotNull(loginResponse);
        Assert.IsFalse(string.IsNullOrWhiteSpace(loginResponse!.Token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
    }
}