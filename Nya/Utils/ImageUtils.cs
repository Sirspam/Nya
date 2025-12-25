using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Animations;
using HMUI;
using IPA.Utilities;
using Nya.Configuration;
using Nya.Entries;
using SiraUtil.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

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
        
        public async Task SetImageViewSprite(ImageView imageView, NyaImageInfo imageInfo, Action? callback)
        {
            if(imageInfo.ImageBytes == null || imageInfo.ImageBytes.Length == 0)
            {
                return;
            }

            _siraLog.Info($"{imageView.name} attempting to load sprite {imageInfo.GetFileName()}");
            Action loggingCallback = () =>
            {
                _siraLog.Info("Sprite loaded!");
                callback?.Invoke();
            };

            // GIF logic taken from BSML's SetImageAsync
            // https://github.com/monkeymanboy/BeatSaberMarkupLanguage/blob/f51845a050f2133a33597202ade73aa102858bd2/BeatSaberMarkupLanguage/BeatSaberUI.cs#L478
            if (imageView.TryGetComponent(out AnimationStateUpdater oldStateUpdater))
            {
                Object.DestroyImmediate(oldStateUpdater);
            }

            if (imageInfo.IsAnimated())
            {
                var stateUpdater = imageView.gameObject.AddComponent<AnimationStateUpdater>();
                stateUpdater.Image = imageView;

                var animationData = await AnimationLoader.ProcessGifAsync(imageInfo.ImageBytes);
                var controllerData = AnimationController.Instance.Register(imageInfo.ImageUrl, animationData);
                stateUpdater.ControllerData = controllerData;
            }
            else
            {
                imageView.sprite = await Utilities.LoadSpriteAsync(imageInfo.ImageBytes);                
            }

            loggingCallback?.Invoke();
        }
    }
}