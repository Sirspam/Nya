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
    public class ImageUtils
    {
        private static readonly HttpClient Client = new HttpClient();

        public static byte[] nyaImageBytes = null;
        public static string nyaImageEndpoint;
        public static string nyaImageURL;

        public static void DownloadNyaImage()
        {
            if (PluginConfig.Instance.Nsfw) File.WriteAllBytes($"{PluginConfig.Instance.LocalFilesPath}/nsfw/{nyaImageEndpoint}", nyaImageBytes);
            else File.WriteAllBytes($"{PluginConfig.Instance.LocalFilesPath}/sfw/{nyaImageEndpoint}", nyaImageBytes);
        }

        public static void CopyNyaImage()
        {
            // Converts gifs to pngs because ???
            // Also doesn't seem to like transparency sometimes
            // Might just remove this feature at some point lmao
            using (MemoryStream ms = new MemoryStream(nyaImageBytes))
            {
                Bitmap bm = new Bitmap(ms);
                Clipboard.SetImage(bm);
            }
        }

#nullable enable

        private static async Task<byte[]?> GetWebDataToBytesAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await Client.GetAsync(url);
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException error)
            {
                Plugin.Log.Error($"Error getting data from {url}, Message: {error}");
                return null;
            }
        }

        private static async Task<string?> GetImageURL(string endpoint)
        {
            try
            {
                Plugin.Log.Debug($"Attempting to get image url from {PluginConfig.Instance.SelectedAPI}");
                var response = await GetWebDataToBytesAsync(WebAPIs.APIs[PluginConfig.Instance.SelectedAPI].URL + endpoint);
                var endpointResult = JsonConvert.DeserializeObject<WebAPIEntries>(Encoding.UTF8.GetString(response));
                nyaImageEndpoint = endpointResult.Url.Split('/').Last();
                return endpointResult.Url;
            }
            catch (Exception)
            {
                return null;
            }
        }

#nullable disable

        public static async Task LoadNyaSprite(ImageView image)
        {
            if (nyaImageBytes == null)
            {
                await LoadNewNyaSprite(image);
                return;
            }
            if (image.sprite.texture.GetRawTextureData() == nyaImageBytes) return;

            Plugin.Log.Info($"Loading image from {nyaImageURL}");
            // Below is essentially BSML's SetImage method but adapted "better" for Nya
            // I didn't like that it would show a yucky loading gif >:(
            AnimationStateUpdater oldStateUpdater = image.GetComponent<AnimationStateUpdater>();
            if (oldStateUpdater != null) UnityEngine.Object.DestroyImmediate(oldStateUpdater);

            if (nyaImageURL.EndsWith(".gif") || nyaImageURL.EndsWith(".apng"))
            {
                AnimationStateUpdater stateUpdater = image.gameObject.AddComponent<AnimationStateUpdater>();
                stateUpdater.image = image;
                AnimationLoader.Process(nyaImageURL.EndsWith(".gif") ? AnimationType.GIF : AnimationType.APNG, nyaImageBytes, (Texture2D tex, Rect[] uvs, float[] delays, int width, int height) =>
                {
                    AnimationControllerData controllerData = AnimationController.instance.Register(nyaImageURL, tex, uvs, delays);
                    stateUpdater.controllerData = controllerData;
                });
            }
            else
            {
                AnimationStateUpdater stateUpdater = image.gameObject.AddComponent<AnimationStateUpdater>();
                stateUpdater.image = image;
                if (stateUpdater != null) UnityEngine.Object.DestroyImmediate(stateUpdater);
                image.sprite = Utilities.LoadSpriteRaw(nyaImageBytes);
            }
        }

        public static async Task LoadNewNyaSprite(ImageView image)
        {
            try
            {
                if (PluginConfig.Instance.SelectedAPI == "Local Files")
                {
                    var type = "sfw";
                    if (PluginConfig.Instance.Nsfw) type = "nsfw";
                    var oldImageURL = nyaImageURL;
                    while (nyaImageURL == oldImageURL)
                    {
                        var files = Directory.GetFiles($"{PluginConfig.Instance.LocalFilesPath}/{type}");
                        var rand = new System.Random();
                        nyaImageURL = files[rand.Next(files.Length)];
                    }
                    Utilities.GetData(nyaImageURL, (byte[] data) => nyaImageBytes = data);
                    await LoadNyaSprite(image);
                    return;
                }

                var selectedEndpoint = PluginConfig.Instance.SelectedEndpoints[PluginConfig.Instance.SelectedAPI].SelectedSfwEndpoint;
                if (PluginConfig.Instance.Nsfw) selectedEndpoint = PluginConfig.Instance.SelectedEndpoints[PluginConfig.Instance.SelectedAPI].SelectedNsfwEndpoint;
                if (WebAPIs.APIs[PluginConfig.Instance.SelectedAPI].json == null) nyaImageURL = WebAPIs.APIs[PluginConfig.Instance.SelectedAPI].URL;
                else
                {
                    var newUrl = nyaImageURL;
                    nyaImageURL = await GetImageURL(selectedEndpoint);
                    while (nyaImageURL == newUrl || nyaImageURL == null)
                    {
                        await Task.Delay(1000);
                        nyaImageURL = await GetImageURL(selectedEndpoint);
                    }
                }
                nyaImageBytes = await GetWebDataToBytesAsync(nyaImageURL);
                await LoadNyaSprite(image);
            }
            catch (Exception e) // e for dEez nuts
            {
                Plugin.Log.Error(e);
                LoadErrorSprite(image);
            }
        }

        private static void LoadErrorSprite(ImageView image)
        {
            Utilities.GetData("Nya.Resources.Chocola_Dead.png", (byte[] data) =>
            {
                nyaImageBytes = data;
                nyaImageURL = "Error Sprite";
                image.sprite = Utilities.LoadSpriteRaw(data);
            });
            Plugin.Log.Warn("Any errors which occur after this was likely caused by the error above this warning");
        }
    }
}