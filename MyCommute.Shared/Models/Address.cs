using System.Text;

namespace MyCommute.Shared.Models
{
    public record Address(string Street, string Number, string ZipCode, string City, string? CountryIso2Code = null)
    {
        public string ToQueryString()
        {
            var builder = new StringBuilder($"{Street} {Number}, {ZipCode} {City}");
            if (!string.IsNullOrEmpty(CountryIso2Code))
            {
                builder.Append($", {CountryIso2Code}");
            }

            return builder.ToString();
        }
    }
}