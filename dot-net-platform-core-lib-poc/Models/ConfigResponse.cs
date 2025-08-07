using System.Text.Json.Serialization;

namespace PlatformCoreLib.Models
{
    public class ConfigResponse
    {
        [JsonPropertyName("propertySources")]
        public List<PropertySource> PropertySources { get; set; } = new();
    }
}