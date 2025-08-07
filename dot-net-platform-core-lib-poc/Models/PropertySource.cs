using System.Text.Json.Serialization;

namespace PlatformCoreLib.Models
{
    public class PropertySource
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public Dictionary<string, string> Source { get; set; } = new();
    }
}