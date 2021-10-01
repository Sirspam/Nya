using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Animations;
using HMUI;
using Newtonsoft.Json;
using Nya.Configuration;
using Nya.Entries;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UnityEngine;

namespace Nya.Utils
{
    internal class ImageUtils
    {
        private static readonly HttpClient client = new HttpClient();
        private static string folderPath = Environment.CurrentDirectory + "/UserData/Nya";

        public static byte[] nyaImageBytes = null;
        public static string nyaImageEndpoint;
        public static string nyaImageURL;

        public static void DownloadNyaImage()
        {
            File.WriteAllBytes($"{folderPath}/{nyaImageEndpoint}", nyaImageBytes);
        }

        public static void CopyNyaImage()
        {
            using (MemoryStream ms = new MemoryStream(nyaImageBytes))
            {
                Bitmap bm = new Bitmap(ms);
                Clipboard.SetImage(bm);
                // Converts gifs to pngs because ???
                // Also really doesn't like transparency
                // Might just remove this feature at some point lmao
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
            Plugin.Log.Debug($"Attempting to get image url from {PluginConfig.Instance.selectedAPI}");
            var response = await GetWebDataToBytesAsync(WebAPIs.APIs[PluginConfig.Instance.selectedAPI].URL + endpoint);
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

        public static async Task LoadNyaSprite(ImageView image)
        {
            if (nyaImageBytes == null)
            {
                await LoadNewNyaSprite(image);
                return;
            }
            if (image.sprite.texture.GetRawTextureData() == nyaImageBytes)
            {
                return;
            }


            Plugin.Log.Info($"Loading image from {nyaImageURL}");
            // Below is essentially BSML's SetImage method but adapted "better" for Nya
            // I didn't like that it would show a yucky loading gif >:(
            AnimationStateUpdater oldStateUpdater = image.GetComponent<AnimationStateUpdater>();
            if (oldStateUpdater != null)
                UnityEngine.Object.DestroyImmediate(oldStateUpdater);

            if (nyaImageURL.EndsWith(".gif") || nyaImageURL.EndsWith(".apng"))
            {
                AnimationStateUpdater stateUpdater = image.gameObject.AddComponent<AnimationStateUpdater>();
                stateUpdater.image = image;
                AnimationLoader.Process((nyaImageURL.EndsWith(".gif") || (nyaImageURL.EndsWith(".gif"))) ? AnimationType.GIF : AnimationType.APNG, nyaImageBytes, (Texture2D tex, Rect[] uvs, float[] delays, int width, int height) =>
                {
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
                image.sprite = Utilities.LoadSpriteRaw(nyaImageBytes);
            }
        }

        public static async Task LoadNewNyaSprite(ImageView image)
        {
            if (PluginConfig.Instance.NSFW) // NSFW
            {
                nyaImageURL = await GetImageURL(PluginConfig.Instance.APIs[PluginConfig.Instance.selectedAPI].selected_NSFW_Endpoint);
            }
            else // SFW
            {
                nyaImageURL = await GetImageURL(PluginConfig.Instance.APIs[PluginConfig.Instance.selectedAPI].selected_SFW_Endpoint);
            }
            if (nyaImageURL == null)
            {
                Utilities.GetData("Nya.Resources.Chocola_Dead.png", (byte[] data) =>
                {
                    nyaImageBytes = data;
                    image.sprite = Utilities.LoadSpriteRaw(data);
                    return;
                });
            }
            nyaImageBytes = await GetWebDataToBytesAsync(nyaImageURL);
            await LoadNyaSprite(image);
        }
    }
}
