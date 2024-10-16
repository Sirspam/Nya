using System;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities.Async;
using Nya.Configuration;
using Nya.Managers;
using Nya.Utils;
using SiraUtil.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Nya.UI.ViewControllers.NyaViewControllers
{
    internal abstract class NyaViewController : IInitializable, IDisposable
    {
        private const float NyaButtonCooldownTime = 1.0f;
        protected static bool AutoNyaActive;
        protected static bool AutoNyaButtonToggle;
        private DateTime _lastNyaPressedTime = DateTime.MinValue;

        private readonly SiraLog _siraLog;
        private readonly ImageUtils _imageUtils;
        protected readonly PluginConfig PluginConfig;
        private readonly AutoNyaTicker _autoNyaTicker;
        private readonly TickableManager _tickableManager;
        private readonly NyaImageManager _nyaImageManager;

        protected NyaViewController(SiraLog siraLog, ImageUtils imageUtils, PluginConfig pluginConfig, TickableManager tickableManager, NyaImageManager nyaImageManager)
        {
            _siraLog = siraLog;
            _imageUtils = imageUtils;
            PluginConfig = pluginConfig;
            _autoNyaTicker = new AutoNyaTicker(nyaImageManager);
            _tickableManager = tickableManager;
            _nyaImageManager = nyaImageManager;
        }

        #region components

        [UIComponent("root")]
        internal readonly RectTransform RootTransform = null!;

        [UIComponent("nya-image")]
        internal readonly ImageView NyaImage = null!;

        [UIComponent("nya-button")]
        internal readonly Button NyaButton = null!;

        [UIComponent("auto-button")] 
        internal readonly Button NyaAutoButton = null!;

        [UIComponent("auto-button")]
        internal readonly TextMeshProUGUI NyaAutoText = null!;

        [UIComponent("settings-button")]
        internal readonly Button NyaSettingsButton = null!;

        [UIComponent("settings-button")]
        internal readonly RectTransform SettingsButtonTransform = null!;

        #endregion components

        #region actions

        [UIAction("#post-parse")]
        protected void NyaPostParse()
        {
            var className = ToString();
            var imageName = className.Substring(className.LastIndexOf(".", StringComparison.Ordinal) + 1).Replace("Controller", "Image");
            NyaImage.name = imageName;
            NyaImage.material = _imageUtils.UIRoundEdgeMaterial;
            SetNyaButtonsInteractability(false);
            _imageUtils.SetImageViewSprite(NyaImage, _nyaImageManager.NyaImageSprite, () =>
            {
                if (_nyaImageManager.NyaImageSprite.name == nameof(Utilities.ImageResources.BlankSprite))
                {
                    return;
                }
                
                UnityMainThreadTaskScheduler.Factory.StartNew(NyaButtonCooldown);
            });
        }

        [UIAction("nya-click")]
        protected async void NyaClicked()
        {
            SetNyaButtonsInteractability(false);
            _lastNyaPressedTime = DateTime.UtcNow;
            await _nyaImageManager.RequestNewNyaImage();
        }

        [UIAction("nya-auto-clicked")]
        protected void AutoNyaClicked()
        {
            AutoNyaButtonToggle = !AutoNyaActive;
            ToggleAutoNya(AutoNyaButtonToggle);
        }

        #endregion actions

        private void NyaImageManagerOnNyaImageChanged(object sender, EventArgs e)
        {
            _siraLog.Info($"Received new Nya image");
            
            Action callback = () => UnityMainThreadTaskScheduler.Factory.StartNew(NyaButtonCooldown);
            
            if (AutoNyaActive)
            {
                callback = () =>
                {
                    AutoNyaTicker.LastNyaTime = DateTime.Now.AddSeconds(PluginConfig.AutoNyaWait);
                    _autoNyaTicker.ImageLoading = false;

                    if (!AutoNyaActive)
                    {
                        NyaButton.interactable = true;
                    }
                };
            }
            
            _imageUtils.SetImageViewSprite(NyaImage, _nyaImageManager.NyaImageSprite, callback);
        }
        
        private void NyaImageManagerOnErrorSpriteLoaded(object sender, EventArgs e)
        {
            if (AutoNyaActive)
            {
                ToggleAutoNya(false);
            }
        }

        private void SetNyaButtonsInteractability(bool value)
        {
            NyaButton.interactable = value;
            NyaAutoButton.interactable = value;
        }

        private async void NyaButtonCooldown()
        {
            if (AutoNyaActive)
            {
                return;
            }
            
            var enableTime = _lastNyaPressedTime.AddSeconds(1.25f);
            await Task.Delay((int) Math.Max(0, (enableTime - DateTime.UtcNow).TotalMilliseconds));
            
            SetNyaButtonsInteractability(false);
            await Task.Delay(TimeSpan.FromSeconds(NyaButtonCooldownTime));
            SetNyaButtonsInteractability(true);
        }

        protected void ToggleAutoNya(bool active)
        {
            switch (active)
            {
                case true:
                    AutoNyaActive = true;
                    _autoNyaTicker.CancellationTokenSource = new CancellationTokenSource();
                    _tickableManager.AddLate(_autoNyaTicker);
                
                    NyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = Color.green;
                    NyaButton.interactable = false;
                    break;
                case false:
                {
                    AutoNyaActive = false;
                    _autoNyaTicker.CancellationTokenSource.Cancel();
                    _tickableManager.RemoveLate(_autoNyaTicker);

                    if (!_autoNyaTicker.ImageLoading)
                    {
                        NyaButton.interactable = true;
                    }

                    NyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f); // Beatgames why 0.502
                    break;
                }
            }
        }
        
        public virtual void Initialize()
        {
            _nyaImageManager.NyaImageChanged += NyaImageManagerOnNyaImageChanged;
            _nyaImageManager.ErrorSpriteLoaded += NyaImageManagerOnErrorSpriteLoaded;
        }

        public virtual void Dispose()
        {
            _nyaImageManager.NyaImageChanged -= NyaImageManagerOnNyaImageChanged;
            _nyaImageManager.ErrorSpriteLoaded -= NyaImageManagerOnErrorSpriteLoaded;
        }
        
        private class AutoNyaTicker : ILateTickable
        {
            public bool ImageLoading;
            public CancellationTokenSource CancellationTokenSource = null!;
            public static DateTime LastNyaTime = DateTime.Now;
            
            private readonly NyaImageManager _nyaImageManager;


            public AutoNyaTicker(NyaImageManager nyaImageManager)
            {
                _nyaImageManager = nyaImageManager;
            }

            public async void LateTick()
            {
                if (LastNyaTime.TimeOfDay < DateTime.Now.TimeOfDay && !ImageLoading)
                {
                    ImageLoading = true;
                    await _nyaImageManager.RequestNewNyaImage(CancellationTokenSource.Token);
                }
            }
        }
    }
}