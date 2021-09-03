using Nya.Configuration;
using Newtonsoft.Json;

namespace Nya.Entries
{
    class WebAPIEntries
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
