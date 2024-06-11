using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nya.Configuration;

namespace Nya.Entries
{
    internal class WebAPIEntries
    {
        [JsonProperty("url")]
        public string? Url { get; set; }

        // Method to get images from Fluxpoint with authentication
        public static async Task<string> GetImageFromFluxpointAsync(string category)
        {
            string apiKey = ConfigManager.GetFluxpointApiKey();
            string url = $"https://gallery.fluxpoint.dev/api/sfw/img/{category}";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
