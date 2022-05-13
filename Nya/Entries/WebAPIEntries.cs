using Newtonsoft.Json;

namespace Nya.Entries
{
    internal class WebAPIEntries
    {
        [JsonProperty("url")]
        public string? Url { get; set; }
    }
}