using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using Newtonsoft.Json;
using Nya.Configuration;
using Nya.Entries;
using SiraUtil.Logging;
using SiraUtil.Web;
using Image = UnityEngine.UI.Image;

namespace Nya.Utils
{
    internal class ImageUtils
    {
        private readonly SiraLog _siraLog;
        private readonly PluginConfig _config;
        private readonly IHttpService _httpService;
        private readonly Random _random;

        private byte[]? _nyaImageBytes;
        private byte[]? _nyaImageBytesCompressed;
        private string? _nyaImageEndpoint;
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
            if (_nyaImageEndpoint == null || _nyaImageBytes == null)
            {
                _siraLog.Error("Failed to download image");
                return;
            }
            
            File.WriteAllBytes(Path.Combine(UnityGame.UserDataPath, "Nya", _config.Nsfw ? "nsfw" : "sfw", _nyaImageEndpoint), _nyaImageBytes);
        }

        public void CopyNyaImage()
        {
            if (_nyaImageBytes == null)
            {
                _siraLog.Error("Failed to copy image");
                return;
            }
            
            // Converts gifs to pngs because ???
            // Also doesn't seem to like transparency sometimes
            // Might just remove this feature at some point lmao
            using var memoryStream = new MemoryStream(_nyaImageBytes);
            var bitmap = new Bitmap(memoryStream);
            Clipboard.SetImage(bitmap);
        }

        private async Task<byte[]?> GetWebDataToBytesAsync(string url)
        {
            try
            {
                var response = await _httpService.GetAsync(url);
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
                var response = await GetWebDataToBytesAsync(WebAPIs.APIs[_config.SelectedAPI].BaseEndpoint + endpoint);
                if (response == null)
                {
                    return null;
                }

                var endpointResult = JsonConvert.DeserializeObject<WebAPIEntries>(Encoding.UTF8.GetString(response));
                _nyaImageEndpoint = endpointResult.Url.Split('/').Last();
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
                if (WebAPIs.APIs[_config.SelectedAPI].Mode == DataMode.Local)
                {
                    var type = _config.Nsfw ? "nsfw" : "sfw";
                    var oldImageURL = NyaImageURL;
                    while (NyaImageURL == oldImageURL)
                    {
                        var files = Directory.GetFiles(Path.Combine(WebAPIs.APIs[_config.SelectedAPI].BaseEndpoint, type));
                        switch (files.Length)
                        {
                            case 0:
                                _siraLog.Warn($"No local files for type: {type}");
                                return;
                            case 1 when oldImageURL != null:
                                return;
                            default:
                                NyaImageURL = files[_random.Next(files.Length)];
                                break;
                        }
                    }

                    _nyaImageBytes = File.ReadAllBytes(NyaImageURL!);
                    await Task.Run(DownscaleNyaImage);
                    LoadNyaImage(image);
                    return;
                }

                var selectedEndpoint = _config.Nsfw
                    ? _config.SelectedEndpoints[_config.SelectedAPI].SelectedNsfwEndpoint
                    : _config.SelectedEndpoints[_config.SelectedAPI].SelectedSfwEndpoint;

                switch (WebAPIs.APIs[_config.SelectedAPI].Mode)
                {
                    case DataMode.Json:
                        var newUrl = NyaImageURL;
                        NyaImageURL = await GetImageURL(selectedEndpoint);
                        while (NyaImageURL == newUrl || NyaImageURL == null)
                        {
                            await Task.Delay(1000);
                            NyaImageURL = await GetImageURL(selectedEndpoint);
                        }

                        break;
                    case DataMode.Unsupported:
                    default:
                        _siraLog.Warn($"Unsupported data mode for endpoint: {_config.SelectedAPI}");
                        return;
                }

                _nyaImageBytes = await GetWebDataToBytesAsync(NyaImageURL);
                await Task.Run(DownscaleNyaImage);
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
            if (_nyaImageBytes == null || NyaImageURL == null)
            {
                return;
            }
            
            var originalImage = System.Drawing.Image.FromStream(new MemoryStream(_nyaImageBytes));
            // This is either a great way to get away with just one comparison or completely stupid
            // Also I simply have no clue how to make this work for gifs, so we'll just leave those be.
            if (originalImage.Width + originalImage.Height <= 1024f || NyaImageURL.EndsWith(".gif") || NyaImageURL.EndsWith(".apng"))
            {
                _nyaImageBytesCompressed = _nyaImageBytes;
                return;
            }

            var ratio = (double)originalImage.Width / originalImage.Height;
            if (512 * ratio <= originalImage.Width)
            {
                var resizedImage = new Bitmap(originalImage, (int)(512f * ratio), 512);
                using var ms = new MemoryStream();
                resizedImage.Save(ms, originalImage.RawFormat);
                _nyaImageBytesCompressed = ms.ToArray();
            }
            else
            {
                var resizedImage = new Bitmap(originalImage, 512, (int)(512 / ratio));
                using var ms = new MemoryStream();
                resizedImage.Save(ms, originalImage.RawFormat);
                _nyaImageBytesCompressed = ms.ToArray();
            }
        }

        public void LoadNyaImage(ImageView image)
        {
            if (_nyaImageBytes == null)
            {
                GetNewNyaImage(image);
                return;
            }

            if (image.sprite.texture.GetRawTextureData() == _nyaImageBytesCompressed)
            {
                return;
            }

            _siraLog.Info($"Loading image from {NyaImageURL}");
            image.SetImage(NyaImageURL, false, true);
            // image.SetImage(NyaImageURL);
        }

        private void LoadErrorSprite(Image image)
        {
            Utilities.GetData("Nya.Resources.Chocola_Dead.png", data =>
            {
                _nyaImageBytes = data;
                NyaImageURL = "Error Sprite";
                image.sprite = Utilities.LoadSpriteRaw(data);
            });
            _siraLog.Warn("Any errors which occur after this was likely caused by the error above this warning");
        }
    }
}