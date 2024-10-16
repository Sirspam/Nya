using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nya.Entries
{
    internal sealed class CommentedImageSourcesEntries
    {
        [JsonProperty("_comment")]
        public string Comment { get; set; } = null!;
        [JsonProperty("Sources")]
        public Dictionary<string, ImageSourceEntry> Sources { get; set; } = null!;
    }
}