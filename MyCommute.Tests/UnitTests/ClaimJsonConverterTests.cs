using System.Linq;
using System.Security.Claims;
using MyCommute.Shared.JsonConverters;
using MyCommute.Shared.Models.Authentication;
using NUnit.Framework;

namespace MyCommute.Tests.UnitTests
{
    [TestFixture]
    public class ClaimJsonConverterTests
    {
        private readonly string loginResponseJson =
            "{\"token\":\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiOGUwNzJhZWEtMmFiNy00YjBiLTg0ZjUtNWVkODk5MzBmZTBmQGludHJhY3RvLmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL3NpZCI6ImM0MzE4YWNlLWMyYWItNGM4ZS00MzJmLTA4ZDk5Mzc5NmM5NiIsIm5iZiI6MTYzNDcwNTgxMCwiZXhwIjoxNjQyNjU0NjEwLCJpYXQiOjE2MzQ3MDU4MTB9.y6_QHoMXWEBxvcbxd3R7zy8X29p3GwuNAvom0wJfFmo\",\"claims\":[{\"issuer\":\"LOCAL AUTHORITY\",\"originalIssuer\":\"LOCAL AUTHORITY\",\"properties\":{},\"subject\":null,\"type\":\"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name\",\"value\":\"8e072aea-2ab7-4b0b-84f5-5ed89930fe0f@intracto.com\",\"valueType\":\"http://www.w3.org/2001/XMLSchema#string\"},{\"issuer\":\"LOCAL AUTHORITY\",\"originalIssuer\":\"LOCAL AUTHORITY\",\"properties\":{},\"subject\":null,\"type\":\"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid\",\"value\":\"c4318ace-c2ab-4c8e-432f-08d993796c96\",\"valueType\":\"http://www.w3.org/2001/XMLSchema#string\"}]}";

        private readonly string claimsArrayJson = "[{\"issuer\":\"LOCAL AUTHORITY\",\"originalIssuer\":\"LOCAL AUTHORITY\",\"properties\":{},\"subject\":null,\"type\":\"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name\",\"value\":\"8e072aea-2ab7-4b0b-84f5-5ed89930fe0f@intracto.com\",\"valueType\":\"http://www.w3.org/2001/XMLSchema#string\"},{\"issuer\":\"LOCAL AUTHORITY\",\"originalIssuer\":\"LOCAL AUTHORITY\",\"properties\":{},\"subject\":null,\"type\":\"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid\",\"value\":\"c4318ace-c2ab-4c8e-432f-08d993796c96\",\"valueType\":\"http://www.w3.org/2001/XMLSchema#string\"}]";

        private readonly string claimJson =
            "{\"issuer\":\"LOCAL AUTHORITY\",\"originalIssuer\":\"LOCAL AUTHORITY\",\"properties\":{},\"subject\":null,\"type\":\"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name\",\"value\":\"8e072aea-2ab7-4b0b-84f5-5ed89930fe0f@intracto.com\",\"valueType\":\"http://www.w3.org/2001/XMLSchema#string\"}";
        
        [Test]
        public void CanDeserializeClaim()
        {
            Assert.Multiple(() =>
            {
                Claim? claim = null;
                Assert.DoesNotThrow(() =>
                {
                    claim = JsonSerializer.Deserialize<Claim>(claimJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new ClaimJsonConverter() }});
                });

                Assert.IsNotNull(claim);
                Assert.AreEqual("LOCAL AUTHORITY", claim!.Issuer);
                Assert.AreEqual("LOCAL AUTHORITY", claim!.OriginalIssuer);
                Assert.AreEqual("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", claim!.Type);
                Assert.AreEqual("8e072aea-2ab7-4b0b-84f5-5ed89930fe0f@intracto.com", claim!.Value);
                Assert.AreEqual("http://www.w3.org/2001/XMLSchema#string", claim!.ValueType);
            });
        }
        
        [Test]
        public void CanDeserializeClaimsArray()
        {
            Assert.Multiple(() =>
            {
                Claim[]? claims = null;
                Assert.DoesNotThrow(() =>
                {
                    claims = JsonSerializer.Deserialize<Claim[]>(claimsArrayJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new ClaimJsonConverter() }});
                });
                
                Assert.IsNotNull(claims);
                Assert.IsTrue(claims!.Any());
            });
        }
        
        [Test]
        public void CanDeserializeLoginResponse()
        {
            Assert.Multiple(() =>
            {
                LoginResponse? response = null;
                Assert.DoesNotThrow(() =>
                {
                    response = JsonSerializer.Deserialize<LoginResponse>(loginResponseJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new ClaimJsonConverter() }});
                });
                
                Assert.IsNotNull(response);
                Assert.IsFalse(string.IsNullOrWhiteSpace(response!.Token));
                Assert.IsTrue(response!.Claims.Any());
            });
        }
    }
}