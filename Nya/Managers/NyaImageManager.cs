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
using UnityEngine;
using Zenject;
using Random = System.Random;

namespace Nya.Managers
{
    internal sealed class NyaImageManager : IInitializable
    {
        private readonly Random _random;
        private readonly SiraLog _siraLog;
        private readonly WebUtils _webUtils;
        private readonly PluginConfig _pluginConfig;
        private readonly ImageSourcesManager _imageSourcesManager;
        
        private byte[] _uncompressedNyaImageBytes = null!;
        private Sprite _nyaImageSprite = null!;
        
        public ref readonly byte[] UncompressedNyaImageBytes => ref _uncompressedNyaImageBytes;
        public ref readonly Sprite NyaImageSprite => ref _nyaImageSprite;

        public event EventHandler? NyaImageChanged;
        public event EventHandler? ErrorSpriteLoaded;
        
        public NyaImageManager(SiraLog siraLog, WebUtils webUtils, PluginConfig pluginConfig, ImageSourcesManager imageSourcesManager)
        {
            _random = new Random();
            _siraLog = siraLog;
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
                byte[]? newImageBytes = null;
                var spriteName = "Nya Sprite";
                
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
                            file.EndsWith(".gif") || file.EndsWith(".apng")).ToArray();
                        if (files.Length == 0)
                        {
                            _siraLog.Error($"No suitable files in folder: {path}");
                            SetErrorSprite();
                            return;
                        }

                        newImageFilePath = files[_random.Next(files.Length)];
                    } while (NyaImageSprite.name == newImageFilePath);

                    newImageBytes = File.ReadAllBytes(newImageFilePath);
                    spriteName = Path.GetFileNameWithoutExtension(newImageFilePath);
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
                                spriteName = Path.GetFileNameWithoutExtension(imageUrl.ToString());
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
                            spriteName = $"image_{timestamp}";
                            break;
                    }
                }
                
                await SetNyaImageSpriteFromBytes(newImageBytes, spriteName);
            }
            catch (Exception exception)
            {
                _siraLog.Error(exception);
                SetErrorSprite();
            }
        }
        
        private async Task SetNyaImageSpriteFromBytes(byte[] imageBytes, string spriteName)
        {
            _uncompressedNyaImageBytes = imageBytes;
            
            var compressedBytes = BeatSaberUI.DownscaleImage(imageBytes, _pluginConfig.ImageScaleValue, _pluginConfig.ImageScaleValue);
            
            var sprite = await Utilities.LoadSpriteAsync(compressedBytes);
            SetNyaImageSprite(sprite, spriteName);
        }

        private void SetNyaImageSprite(Sprite sprite, string spriteName)
        {
            _nyaImageSprite = sprite;
            _nyaImageSprite.name = spriteName;
            NyaImageChanged?.Invoke(this, EventArgs.Empty);
        }
        
        private async void SetErrorSprite()
        {
            var sprite = await Utilities.LoadSpriteFromAssemblyAsync(Assembly.GetExecutingAssembly(), "Nya.Resources.Chocola_Dead.png");
            SetNyaImageSprite(sprite, "Error Sprite");
            
            _siraLog.Warn("Error sprite set");
            ErrorSpriteLoaded?.Invoke(this, EventArgs.Empty);
        }

        public async void Initialize()
        {
            _nyaImageSprite = Utilities.ImageResources.BlankSprite;
            _nyaImageSprite.name = nameof(Utilities.ImageResources.BlankSprite);
            
            await RequestNewNyaImage();
        }
    }
}