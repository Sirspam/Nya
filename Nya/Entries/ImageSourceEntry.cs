using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nya.Entries
{
    internal sealed class ImageSourceEntry
    {
        internal enum ResponseTypeEnum
        {
            URL,
            Image,
        }
        
        /// <summary>
        /// The URL to request the image from
        /// </summary>
        [JsonProperty("Url")]
        [JsonRequired]
        public string Url { get; set; } = null!;

        /// <summary>
        /// The expected response when requesting an image
        /// (URL to image or the image itself)
        /// </summary>
        [JsonProperty("ResponseType")]
        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter), true)]
        public ResponseTypeEnum ResponseType { get; set; }
        
        /// <summary>
        /// Specifies the entry in the API response that contains the URL to the image
        /// Only required if ResponseType is URL
        /// </summary>
        [JsonProperty("UrlResponseEntry")]
        public string? UrlResponseEntry { get; set; }
        
        /// <summary>
        /// !!! NOT TESTED !!! Please report any issues
        /// Optional property, specifies a token to use when making requests to the API
        /// Intended for authentication purposes
        /// </summary>
        [JsonProperty("Token")]
        public string? Token { get; set; }
        
        /// <summary>
        /// !!! NOT TESTED !!! Please report any issues
        /// Optional property, specifies query parameters to append to the URL when making requests to the API
        /// Intended for authentication purposes
        /// </summary>
        [JsonProperty("QueryParameters")]
        public string? QueryParameters { get; set; }
        
        /// <summary>
        /// List of SFW endpoints which the API supports
        /// </summary>
        [JsonProperty("SfwEndpoints")]
        [JsonRequired]
        public List<string> SfwEndpoints { get; set; } = null!;

        /// <summary>
        /// List of NSFW endpoints which the API supports
        /// List can be empty but is required
        /// </summary>
        [JsonProperty("NsfwEndpoints")]
        [JsonRequired]
        public List<string> NsfwEndpoints { get; set; } = null!;
        
        /// <summary>
        /// Indicates if the source is saved locally on the user's machine.
        /// </summary>
        public bool IsLocal { get; set; } = false;

        public List<string> GetSelectedEndpoints(bool nsfw)
        {
            return nsfw ? NsfwEndpoints : SfwEndpoints;
        }
    }
}