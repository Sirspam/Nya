using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Animations;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Nya.Configuration;
using Nya.Entries;
using UnityEngine;
using HMUI;

namespace Nya.Utils
{
    public class ImageUtils
    {
        private static readonly HttpClient client = new HttpClient();
        private static byte[] nyaImageBytes;
        private static string nyaImageEndpoint;
        private static string nyaImageURL;
        private static string folderPath = Environment.CurrentDirectory + "/UserData/Nya";

        public static void downloadNyaImage()
        {
            File.WriteAllBytes($"{folderPath}/{nyaImageEndpoint}", nyaImageBytes);
        }
        public static void copyNyaImage()
        {
            using (MemoryStream ms = new MemoryStream(nyaImageBytes))
            {
                Bitmap bm = new Bitmap(ms);
                Clipboard.SetImage(bm); // Converts gifs to pngs because ???
            }
        }
        #nullable enable
        public static async Task<byte[]?> GetWebDataToBytesAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException error)
            {
                Plugin.Log.Error($"Error getting data from {url}, Message: {error}");
                return null;
            }
        }

        public static async Task<string?> GetImageURL(string endpoint)
        {
            var response = await GetWebDataToBytesAsync(PluginConfig.Instance.APIs[PluginConfig.Instance.selectedAPI].URL + endpoint);
            if (response == null)
            {
                return null;
            }
            var endpointResult = JsonConvert.DeserializeObject<WebAPIEntries>(Encoding.UTF8.GetString(response));
            if (endpointResult == null)
            {
                return null;
            }
            nyaImageEndpoint = endpointResult.Url.Split('/').Last();
            return endpointResult.Url;
        }
        #nullable disable

        public static async Task loadNewNyaSprite(ImageView image)
        {
            if (PluginConfig.Instance.NSFW) // NSFW
            {
                nyaImageURL = await GetImageURL(PluginConfig.Instance.APIs[PluginConfig.Instance.selectedAPI].selectedNSFW_Endpoint);
            }
            else // SFW
            {
                nyaImageURL = await GetImageURL(PluginConfig.Instance.APIs[PluginConfig.Instance.selectedAPI].selectedSFW_Endpoint);
            }
            if (nyaImageURL == null)
            {
                Utilities.GetData("Nya.Assets.Chocola_Dead.png", (byte[] data) =>
                {
                    nyaImageBytes = data;
                    image.sprite = Utilities.LoadSpriteRaw(data);
                    return;
                });
            }
            Plugin.Log.Debug($"Loading from {nyaImageURL}");

            // Below is essentially BSML's SetImage method but adapted "better" for Nya
            // I didn't like that it would show a yucky loading gif >:(
            AnimationStateUpdater oldStateUpdater = image.GetComponent<AnimationStateUpdater>();
            if (oldStateUpdater != null)
                UnityEngine.Object.DestroyImmediate(oldStateUpdater);

            if (nyaImageURL.EndsWith(".gif") || nyaImageURL.EndsWith(".apng"))
            {
                AnimationStateUpdater stateUpdater = image.gameObject.AddComponent<AnimationStateUpdater>();
                stateUpdater.image = image;
                var data = await GetWebDataToBytesAsync(nyaImageURL);
                AnimationLoader.Process((nyaImageURL.EndsWith(".gif") || (nyaImageURL.EndsWith(".gif"))) ? AnimationType.GIF : AnimationType.APNG, data, (Texture2D tex, Rect[] uvs, float[] delays, int width, int height) =>
                {
                    nyaImageBytes = data;
                    AnimationControllerData controllerData = AnimationController.instance.Register(nyaImageURL, tex, uvs, delays);
                    stateUpdater.controllerData = controllerData;
                });
            }
            else
            {
                AnimationStateUpdater stateUpdater = image.gameObject.AddComponent<AnimationStateUpdater>();
                stateUpdater.image = image;
                if (stateUpdater != null)
                    UnityEngine.Object.DestroyImmediate(stateUpdater);
                var data = await GetWebDataToBytesAsync(nyaImageURL);
                nyaImageBytes = data;
                image.sprite = Utilities.LoadSpriteRaw(data);
            }
        }
    }
}
