using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Animations;
using HMUI;
using IPA.Utilities;
using Newtonsoft.Json;
using Nya.Configuration;
using Nya.Entries;
using SiraUtil.Logging;
using SiraUtil.Web;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Nya.Utils
{
    internal class ImageUtils
    {
        public static event Action? ErrorSpriteLoadedEvent;
        
        private byte[]? _nyaImageBytes;
        private string? _nyaImageEndpoint;
        private string? _nyaImageURL;

        private readonly Random _random;
        private readonly SiraLog _siraLog;
        private readonly IHttpService _httpService;
        private readonly PluginConfig _pluginConfig;

        public ImageUtils(SiraLog siraLog, IHttpService httpService, PluginConfig pluginConfig)
        {
            _random = new Random();
            _siraLog = siraLog;
            _httpService = httpService;
            _pluginConfig = pluginConfig;
        }
        
        public void DownloadNyaImage()
        {
            if (_nyaImageEndpoint == null || _nyaImageBytes == null)
            {
                _siraLog.Error("Failed to download image");
                return;
            }
            
            File.WriteAllBytes(Path.Combine(UnityGame.UserDataPath, "Nya", _pluginConfig.NsfwImages ? "nsfw" : "sfw", _nyaImageEndpoint), _nyaImageBytes);
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
                var path = ImageSources.Sources[_pluginConfig.SelectedAPI].BaseEndpoint + endpoint;
                _siraLog.Info($"Attempting to get image url from {path}");
                var response = await GetWebDataToBytesAsync(path);
                if (response == null)
                {
                    return null;
                }

                var endpointResult = JsonConvert.DeserializeObject<WebAPIEntries>(Encoding.UTF8.GetString(response));
                if (endpointResult.Url == null)
                {
                    _siraLog.Error($"Couldn't find url value in response: {JsonConvert.SerializeObject(Encoding.UTF8.GetString(response))}");
                    return null;
                }
                
                _nyaImageEndpoint = endpointResult.Url.Split('/').Last();
                return endpointResult.Url;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async void LoadNewNyaImage(ImageView image, Action? callback)
        {
            try
            {
                var selectedEndpoint = _pluginConfig.NsfwImages
                    ? _pluginConfig.SelectedEndpoints[_pluginConfig.SelectedAPI].SelectedNsfwEndpoint
                    : _pluginConfig.SelectedEndpoints[_pluginConfig.SelectedAPI].SelectedSfwEndpoint;

                switch (ImageSources.Sources[_pluginConfig.SelectedAPI].Mode)
                {
                    case DataMode.Json:
                        var newUrl = _nyaImageURL;
                        _nyaImageURL = await GetImageURL(selectedEndpoint);
                        var count = 0;
                        while (_nyaImageURL == newUrl || _nyaImageURL == null)
                        {
                            count += 1;
                            if (count == 3)
                            {
                                if (_nyaImageURL == null)
                                {
                                    LoadErrorSprite(image);
                                    callback?.Invoke();
                                    return;
                                }
                                
                                break;
                            }

                            await Task.Delay(1000);
                            _nyaImageURL = await GetImageURL(selectedEndpoint);
                        }

                        break;
                    case DataMode.Local:
                        var type = _pluginConfig.NsfwImages ? "nsfw" : "sfw";
                        var oldImageURL = _nyaImageURL;
                        while (_nyaImageURL == oldImageURL)
                        {
                            var files = Directory.GetFiles(Path.Combine(ImageSources.Sources[_pluginConfig.SelectedAPI].BaseEndpoint, type));
                            switch (files.Length)
                            {
                                case 0:
                                    _siraLog.Error($"No local files for type: {type}");
                                    LoadErrorSprite(image);
                                    return;
                                case 1 when oldImageURL != null:
                                    return;
                                default:
                                    _nyaImageURL = files[_random.Next(files.Length)];
                                    break;
                            }
                        }
                        
                        _nyaImageBytes = File.ReadAllBytes(_nyaImageURL!);
                        break;
                    case DataMode.Unsupported:
                    default:
                        _siraLog.Warn($"Unsupported data mode for endpoint: {_pluginConfig.SelectedAPI}");
                        return;
                }

                _nyaImageBytes = await GetWebDataToBytesAsync(_nyaImageURL!);
                LoadCurrentNyaImage(image, () => callback?.Invoke());
            }
            catch (Exception e) // e for dEez nuts
            {
                _siraLog.Error(e);
                LoadErrorSprite(image);
            }
        }
        
        public void LoadCurrentNyaImage(ImageView image, Action? callback)
        {
            if (_nyaImageBytes == null)
            {
                LoadNewNyaImage(image, callback);
                return;
            }
            
            if (image.name == _nyaImageURL)
            {
                callback?.Invoke();
                return;
            }

            _siraLog.Info($"Loading image from {_nyaImageURL}");
            var options = new BeatSaberUI.ScaleOptions
            {
                ShouldScale = _pluginConfig.ImageScaleValue != 0,
                MaintainRatio = true,
                Width = _pluginConfig.ImageScaleValue,
                Height = _pluginConfig.ImageScaleValue
            };
            image.SetImage(_nyaImageURL, false, options, () => callback?.Invoke());
            image.name = _nyaImageURL;
        }

        private void LoadErrorSprite(Image image)
        {
            var oldStateUpdater = image.GetComponent<AnimationStateUpdater>();
            if (oldStateUpdater != null)
                Object.DestroyImmediate(oldStateUpdater);
            
            Utilities.GetData("Nya.Resources.Chocola_Dead.png", data =>
            {
                _nyaImageBytes = data;
                _nyaImageURL = "Error Sprite";
                image.sprite = Utilities.LoadSpriteRaw(data);
                image.name = _nyaImageURL;
            });
            _siraLog.Warn("Error sprite loaded");
            ErrorSpriteLoadedEvent?.Invoke();
        }
    }
}