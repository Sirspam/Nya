using System;
using System.IO;
using System.Linq;
using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using Nya.Configuration;
using SiraUtil.Logging;
using UnityEngine;

namespace Nya.Utils
{
    internal class ImageUtils
    {
        private Material? _uiRoundEdgeMaterial;
        
        private readonly SiraLog _siraLog;
        private readonly PluginConfig _pluginConfig;

        public ImageUtils(SiraLog siraLog, PluginConfig pluginConfig)
        {
            _siraLog = siraLog;
            _pluginConfig = pluginConfig;
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
        
        public void SaveImageBytesToDisk(byte[] bytes, string fileName)
        {
            File.WriteAllBytes(Path.Combine(UnityGame.UserDataPath, "Nya", _pluginConfig.NsfwImages ? "nsfw" : "sfw", fileName), bytes);
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
        }

        public byte[] DownscaleImageBytes(byte[] imageBytes)
        {
            return BeatSaberUI.DownscaleImage(imageBytes, _pluginConfig.ImageScaleValue, _pluginConfig.ImageScaleValue);
        }
    }
}