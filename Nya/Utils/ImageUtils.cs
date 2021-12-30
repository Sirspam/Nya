using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Animations;
using HMUI;
using IPA.Utilities;
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
using SiraUtil.Logging;
using SiraUtil.Web;

namespace Nya.Utils
{
    internal class ImageUtils
    {
        private readonly SiraLog _siraLog;
        private readonly PluginConfig _config;
        private readonly IHttpService _httpService;
        private readonly Random _random;

        public byte[]? NyaImageBytes;
        public byte[]? NyaImageBytesCompressed;
        public string? NyaImageEndpoint;
        public string? NyaImageURL;

        public ImageUtils(SiraLog siraLog, PluginConfig config, IHttpService httpService)
        {
            _siraLog = siraLog;
            _config = config;
            _httpService = httpService;
            
            _random = new Random();
        }

        public void DownloadNyaImage()
        {
            File.WriteAllBytes(Path.Combine(UnityGame.UserDataPath, "Nya", _config.Nsfw ? "nsfw" : "sfw", NyaImageEndpoint), NyaImageBytes);
        }

        public void CopyNyaImage()
        {
            // Converts gifs to pngs because ???
            // Also doesn't seem to like transparency sometimes
            // Might just remove this feature at some point lmao
            using MemoryStream memoryStream = new MemoryStream(NyaImageBytes);
            Bitmap bitmap = new Bitmap(memoryStream);
            Clipboard.SetImage(bitmap);
        }

        private async Task<byte[]?> GetWebDataToBytesAsync(string url)
        {
            try
            {
                IHttpResponse response = await _httpService.GetAsync(url);
                if (!response.Successful)
                {
                    _siraLog.Error($"{url} returned an unsuccessful status code ({response.Code.ToString()})");
                    return null;
                }

                return await response.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException error)
            {
                _siraLog.Error($"Error getting data from {url}, Message: {error}");
                return null;
            }
        }

        private async Task<string?> GetImageURL(string endpoint)
        {
            try
            {
                _siraLog.Info($"Attempting to get image url from {_config.SelectedAPI}, {endpoint}");
                var response = await GetWebDataToBytesAsync(WebAPIs.APIs[_config.SelectedAPI].URL + endpoint);
                var endpointResult = JsonConvert.DeserializeObject<WebAPIEntries>(Encoding.UTF8.GetString(response));
                NyaImageEndpoint = endpointResult.Url.Split('/').Last();
                return endpointResult.Url;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async void GetNewNyaImage(ImageView image)
        {
            try
            {
                if (_config.SelectedAPI == "Local Files")
                {
                    var type = "sfw";
                    if (_config.Nsfw) type = "nsfw";
                    var oldImageURL = NyaImageURL;
                    while (NyaImageURL == oldImageURL)
                    {
                        var files = Directory.GetFiles(Path.Combine(UnityGame.UserDataPath, "Nya", type));
                        if (files.Length == 1 && oldImageURL != null)
                            return;
                        NyaImageURL = files[_random.Next(files.Length)];
                    }
                    Utilities.GetData(NyaImageURL, data => NyaImageBytes = data);
                    await Task.Run(() => DownscaleNyaImage());
                    LoadNyaImage(image);
                    return;
                }

                var selectedEndpoint = _config.SelectedEndpoints[_config.SelectedAPI].SelectedSfwEndpoint;
                if (_config.Nsfw)
                    selectedEndpoint = _config.SelectedEndpoints[_config.SelectedAPI].SelectedNsfwEndpoint;
                if (WebAPIs.APIs[_config.SelectedAPI].json == null) 
                    NyaImageURL = WebAPIs.APIs[_config.SelectedAPI].URL;
                else
                {
                    var newUrl = NyaImageURL;
                    NyaImageURL = await GetImageURL(selectedEndpoint);
                    while (NyaImageURL == newUrl || NyaImageURL == null)
                    {
                        await Task.Delay(1000);
                        NyaImageURL = await GetImageURL(selectedEndpoint);
                    }
                }
                NyaImageBytes = await GetWebDataToBytesAsync(NyaImageURL);
                await Task.Run(() => DownscaleNyaImage());
                LoadNyaImage(image);
            }
            catch (Exception e) // e for dEez nuts
            {
                _siraLog.Error(e);
                LoadErrorSprite(image);
            }
        }

        private void DownscaleNyaImage()
        {      
            var originalImage = Image.FromStream(new MemoryStream(NyaImageBytes));
            // This is either a great way to get away with just one comparison or completely stupid
            // Also I simply have no clue how to make this work for gifs, so we'll just leave those be.
            if ((originalImage.Width + originalImage.Height) <= 1024f || NyaImageURL.EndsWith(".gif") || NyaImageURL.EndsWith(".apng"))
            {
                NyaImageBytesCompressed = NyaImageBytes;
                return;
            }

            double ratio = (double)originalImage.Width / originalImage.Height;
            if ((512 * ratio) <= originalImage.Width)
            {
                var resizedImage = new Bitmap(originalImage, (int)(512f * ratio), 512);
                using MemoryStream ms = new MemoryStream();
                resizedImage.Save(ms, originalImage.RawFormat);
                NyaImageBytesCompressed = ms.ToArray();
            }
            else
            {
                var resizedImage = new Bitmap(originalImage, 512, (int)(512 / ratio));
                using MemoryStream ms = new MemoryStream();
                resizedImage.Save(ms, originalImage.RawFormat);
                NyaImageBytesCompressed = ms.ToArray();
            }
        }

        public void LoadNyaImage(ImageView image)
        {
            if (NyaImageBytes == null)
            {
                GetNewNyaImage(image);
                return;
            }
            if (image.sprite.texture.GetRawTextureData() == NyaImageBytesCompressed) return;

            _siraLog.Info($"Loading image from {NyaImageURL}");

            // Below is essentially BSML's SetImage method but adapted "better" for Nya
            // I didn't like that it would show a yucky loading gif >:(
            AnimationStateUpdater oldStateUpdater = image.GetComponent<AnimationStateUpdater>();
            if (oldStateUpdater != null) UnityEngine.Object.DestroyImmediate(oldStateUpdater);

            if (NyaImageURL.EndsWith(".gif") || NyaImageURL.EndsWith(".apng"))
            {
                AnimationStateUpdater stateUpdater = image.gameObject.AddComponent<AnimationStateUpdater>();
                stateUpdater.image = image;

                AnimationLoader.Process(NyaImageURL.EndsWith(".gif") ? AnimationType.GIF : AnimationType.APNG, NyaImageBytes, (tex, uvs, delays, width, height) =>
                {
                    AnimationControllerData controllerData = AnimationController.instance.Register(NyaImageURL, tex, uvs, delays);
                    stateUpdater.controllerData = controllerData;
                });
            }
            else
            {
                AnimationStateUpdater stateUpdater = image.gameObject.AddComponent<AnimationStateUpdater>();
                stateUpdater.image = image;
                if (stateUpdater != null) 
                    UnityEngine.Object.DestroyImmediate(stateUpdater);
                image.sprite = Utilities.LoadSpriteRaw(NyaImageBytesCompressed);
            }
        }

        private void LoadErrorSprite(ImageView image)
        {
            Utilities.GetData("Nya.Resources.Chocola_Dead.png", data =>
            {
                NyaImageBytes = data;
                NyaImageURL = "Error Sprite";
                image.sprite = Utilities.LoadSpriteRaw(data);
            });
            _siraLog.Warn("Any errors which occur after this was likely caused by the error above this warning");
        }
    }
}