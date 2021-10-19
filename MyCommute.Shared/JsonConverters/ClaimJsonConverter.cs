using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyCommute.Shared.JsonConverters
{
    public class ClaimJsonConverter : JsonConverter<Claim>
    {
        public override Claim? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var type = string.Empty;          
            var value = string.Empty;         
            var valueType = string.Empty;     
            var issuer = string.Empty;        
            var originalIssuer = string.Empty;
            ClaimsIdentity? subject = null;
            
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("JSON payload expected to start with StartObject token.");
            }
            
            var startDepth = reader.CurrentDepth;
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == startDepth)
                {
                    return new Claim(type, value, valueType, issuer, originalIssuer, subject);
                }

                if (reader.TokenType != JsonTokenType.PropertyName) continue;
                var propertyName = reader.GetString();
                reader.Read();
                switch (propertyName)
                {
                    case "Properties":
                    case "properties":
                        // TODO
                        break;
                    case "Subject":
                    case "subject":
                        // TODO
                        // reader.Read();
                        // if (reader.TokenType == JsonTokenType.String)
                        // {
                        //     var subjectJson = reader.GetString();
                        //     if (!string.IsNullOrWhiteSpace(subjectJson))
                        //     {
                        //         subject = JsonSerializer.Deserialize<ClaimsIdentity>(subjectJson);
                        //     }
                        // }
                        break;
                    case "Type":
                    case "type":
                        type = reader.GetString() ?? throw new JsonException(
                            $"\"{propertyName}\" can not be null.");
                        break;
                    case "Value":
                    case "value":
                        value = reader.GetString() ?? throw new JsonException(
                            $"\"{propertyName}\" can not be null.");;
                        break;
                    case "ValueType":
                    case "valueType":
                        valueType = reader.GetString();
                        break;
                    case "Issuer":
                    case "issuer":
                        issuer = reader.GetString();
                        break;
                    case "OriginalIssuer":
                    case "originalIssuer":
                        originalIssuer = reader.GetString();
                        break;
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Claim value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}