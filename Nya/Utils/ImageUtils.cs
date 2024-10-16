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
using Nya.Managers;
using SiraUtil.Logging;
using SiraUtil.Web;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Nya.Utils
{
    internal class ImageUtils
    {
        private Material? _uiRoundEdgeMaterial;

        private readonly Random _random;
        private readonly SiraLog _siraLog;
        private readonly IHttpService _httpService;
        private readonly PluginConfig _pluginConfig;
        private readonly ImageSourcesManager _imageSourcesManager;

        public ImageUtils(SiraLog siraLog, IHttpService httpService, PluginConfig pluginConfig, ImageSourcesManager imageSourcesManager)
        {
            _random = new Random();
            _siraLog = siraLog;
            _httpService = httpService;
            _pluginConfig = pluginConfig;
            _imageSourcesManager = imageSourcesManager;
        }

        public Material UIRoundEdgeMaterial
        {
            get
            {
                if (_uiRoundEdgeMaterial is null)
                {
                    _uiRoundEdgeMaterial = Resources.FindObjectsOfTypeAll<Material>().Last(x => x.name == "UINoGlowRoundEdge");
                }

                return _uiRoundEdgeMaterial;
            }
        }
        
        public void SaveSpriteImage(Sprite sprite)
        {
            var bytes = sprite.texture.EncodeToPNG();
            
            File.WriteAllBytes(Path.Combine(UnityGame.UserDataPath, "Nya", _pluginConfig.NsfwImages ? "nsfw" : "sfw", $"{sprite.name}.png"), bytes);
        }
        
        public void SetImageViewSprite(ImageView imageView, Sprite sprite, Action? callback)
        {
            _siraLog.Info($"{imageView.name} attempting to load sprite {sprite.name}");
            Action loggingCallback = () =>
            {
                _siraLog.Info("Sprite loaded!");
                callback?.Invoke();
            };

            imageView.sprite = sprite;
            loggingCallback?.Invoke();
            
            // TODO: Implement downscaling
            
            // var options = new BeatSaberUI.ScaleOptions
            // {
            //     ShouldScale = _pluginConfig.ImageScaleValue != 0,
            //     MaintainRatio = true,
            //     Width = _pluginConfig.ImageScaleValue,
            //     Height = _pluginConfig.ImageScaleValue
            // };
            // imageView.SetImage(sprite.texture, false, options, () => callback?.Invoke());
        }
    }
}