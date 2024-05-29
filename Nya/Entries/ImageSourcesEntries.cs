using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nya.Entries
{
    internal class ImageSourceEntry
    {
        [JsonProperty("Url")]
        public string Url { get; set; } = null!;

        public bool IsLocal { get; set; } = false;
        
        [JsonProperty("SfwEndpoints")]
        public List<string> SfwEndpoints { get; set; } = null!;

        [JsonProperty("NsfwEndpoints")]
        public List<string> NsfwEndpoints { get; set; } = null!;
    }
}