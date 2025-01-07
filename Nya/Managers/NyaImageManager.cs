using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using Newtonsoft.Json.Linq;
using Nya.Configuration;
using Nya.Entries;
using Nya.Utils;
using SiraUtil.Logging;
using Zenject;
using Random = System.Random;

namespace Nya.Managers
{
    internal sealed class NyaImageManager : IInitializable
    {
        private readonly Random _random;
        private readonly SiraLog _siraLog;
        private NyaImageInfo _nyaImageInfo;
        private readonly WebUtils _webUtils;
        private readonly PluginConfig _pluginConfig;
        private readonly ImageSourcesManager _imageSourcesManager;

        public NyaImageInfo NyaImageInfo
        {
            get => _nyaImageInfo;
            private set
            {
                _nyaImageInfo = value;
                NyaImageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler? NyaImageChanged;
        public event EventHandler? ErrorSpriteLoaded;
        
        public NyaImageManager(SiraLog siraLog, WebUtils webUtils, PluginConfig pluginConfig, ImageSourcesManager imageSourcesManager)
        {
            _random = new Random();
            _siraLog = siraLog;

            var blankSpriteBytes = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=");
            _nyaImageInfo = new NyaImageInfo(blankSpriteBytes, "Blank.png");
            _webUtils = webUtils;
            _pluginConfig = pluginConfig;
            _imageSourcesManager = imageSourcesManager;
        }

        public async Task RequestNewNyaImage(CancellationToken? cancellationToken = null)
        {
            cancellationToken ??= CancellationToken.None;
            
            var sources = await _imageSourcesManager.GetSourcesDictionary();

            string selectedApi;
            string selectedEndpoint;

            if (_pluginConfig.IsAprilFirst)
            {
                selectedApi = "nekos.life";
                selectedEndpoint = "woof";
            }
            else
            {
                selectedApi = _pluginConfig.SelectedAPI;
                selectedEndpoint = _pluginConfig.GetSelectedEndpoint();
            }
            
            var imageSource = sources[selectedApi];
            
            if (selectedEndpoint.ToLower() == "random")
            {
                var endpoints = imageSource.GetSelectedEndpoints(_pluginConfig.NsfwImages);
                selectedEndpoint = endpoints[_random.Next(endpoints.Count)];
            }
            
            try
            {
                byte[]? newImageBytes;
                string spritePath;
                
                if (imageSource.IsLocal)
                {
                    var folder = _pluginConfig.NsfwImages ? "nsfw" : "sfw";

                    if (selectedEndpoint != folder)
                    {
                        folder = Path.Combine(folder, selectedEndpoint);
                    }

                    string newImageFilePath;
                    do
                    {
                        var path = Path.Combine(sources[_pluginConfig.SelectedAPI].Url, folder);
                        var files = Directory.GetFiles(path).Where(file =>
                            file.EndsWith(".png") || file.EndsWith(".jpeg") || file.EndsWith(".jpg") ||
                            file.EndsWith(".gif")).ToArray();
                        if (files.Length == 0)
                        {
                            _siraLog.Error($"No suitable files in folder: {path}");
                            SetErrorSprite();
                            return;
                        }

                        newImageFilePath = files[_random.Next(files.Length)];
                        _siraLog.Info(NyaImageInfo.ImageUrl);
                        _siraLog.Info(newImageFilePath);
                    } while (NyaImageInfo.ImageUrl == newImageFilePath);

                    newImageBytes = File.ReadAllBytes(newImageFilePath);
                    spritePath = newImageFilePath;
                }
                else
                {
                    var url = Path.Combine(imageSource.Url, selectedEndpoint);
                    _siraLog.Info($"Requesting image from {url}");
                    
                    switch (imageSource.ResponseType)
                    {
                        case ImageSourceEntry.ResponseTypeEnum.URL:
                            var httpResponse = await _webUtils.GetAsync(url, cancellationToken.Value);
                            var jsonObject = await _webUtils.ParseWebResponse<JObject>(httpResponse!);
                            if (jsonObject is not null && jsonObject.TryGetValue(imageSource.UrlResponseEntry!, out var imageUrl))
                            {
                                _siraLog.Info(imageUrl.ToString());
                                var urlResponse = await _webUtils.GetAsync(imageUrl.ToString(), cancellationToken.Value);

                                if (urlResponse is null)
                                {
                                    _siraLog.Error("Failed to get image from URL");
                                    return;
                                }
                                
                                newImageBytes = await urlResponse.ReadAsByteArrayAsync();
                                spritePath = imageUrl.ToString();
                            }
                            else
                            {
                                _siraLog.Error("Failed to get image URL from JSON response");
                                if (jsonObject != null)
                                {
                                    _siraLog.Info($"Couldn't find entry {imageSource.UrlResponseEntry} in JSON object");
                                    _siraLog.Debug(jsonObject.ToString());
                                }
                                SetErrorSprite();
                                return;
                            }
                            break;
                        default:
                        case ImageSourceEntry.ResponseTypeEnum.Image:
                            var imageResponse = await _webUtils.GetAsync(url, cancellationToken.Value);
                            
                            if (imageResponse is null)
                            {
                                _siraLog.Error("Failed to get image from URL");
                                return;
                            }

                            newImageBytes = await imageResponse.ReadAsByteArrayAsync();
                            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
                            spritePath = $"image_{timestamp}.png";
                            break;
                    }
                }
                
                SetNyaImageInfo(newImageBytes, spritePath);
            }
            catch (Exception exception)
            {
                _siraLog.Error(exception);
                SetErrorSprite();
            }
        }
        
        private void SetNyaImageInfo(byte[] imageBytes, string imageUrl)
        {
            NyaImageInfo = new NyaImageInfo(imageBytes, imageUrl);
        }
        
        private async void SetErrorSprite()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("Nya.Resources.Chocola_Dead.png"))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream!.CopyToAsync(memoryStream);
                    SetNyaImageInfo(memoryStream.ToArray(), "Chocola_Dead.png");
                }
            }
            
            _siraLog.Warn("Error sprite set");
            ErrorSpriteLoaded?.Invoke(this, EventArgs.Empty);
        }

        public async void Initialize()
        {
            await RequestNewNyaImage();
        }
    }
}