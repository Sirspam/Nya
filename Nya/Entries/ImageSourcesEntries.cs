using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nya.Entries
{
    internal class ImageSourceEntry
    {
        internal enum ResponseTypeEnum
        {
            Url,
            Image
        }
        
        /// <summary>
        /// The URL to request the image from
        /// </summary>
        [JsonProperty("Url")]
        public string Url { get; set; } = null!;

        /// <summary>
        /// The expected response when requesting an image
        /// (URL to image or the image itself)
        /// </summary>
        [JsonProperty("ResponseType")]
        public ResponseTypeEnum ResponseType { get; set; }
        
        /// <summary>
        /// List of SFW endpoints which the API supports
        /// </summary>
        [JsonProperty("SfwEndpoints")]
        public List<string> SfwEndpoints { get; set; } = null!;

        /// <summary>
        /// List of NSFW endpoints which the API supports
        /// </summary>
        [JsonProperty("NsfwEndpoints")]
        public List<string> NsfwEndpoints { get; set; } = null!;
        
        /// <summary>
        /// Indicates if the source is saved locally on the user's machine.
        /// </summary>
        public bool IsLocal { get; set; } = false;
    }
}