using System;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Nya.UI.ViewControllers.NyaViewControllers
{
    internal abstract class NyaViewController
    {
        protected static bool AutoNyaActive;
        protected static bool AutoNyaButtonToggle;

        protected readonly ImageUtils ImageUtils;
        protected readonly PluginConfig PluginConfig;
        private readonly AutoNyaManager _autoNyaManager;
        private readonly TickableManager _tickableManager;
        
        protected NyaViewController(ImageUtils imageUtils, PluginConfig pluginConfig, TickableManager tickableManager)
        {
            ImageUtils = imageUtils;
            PluginConfig = pluginConfig;
            _autoNyaManager = new AutoNyaManager(ImageUtils, PluginConfig, this);
            _tickableManager = tickableManager;
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
            NyaButton.interactable = false;
            ImageUtils.LoadCurrentNyaImage(NyaImage, () => NyaButton.interactable = true);
        }

        [UIAction("nya-click")]
        protected void NyaClicked()
        {
            NyaButton.interactable = false;
            ImageUtils.LoadNewNyaImage(NyaImage, () => NyaButton.interactable = true);
        }

        [UIAction("nya-auto-clicked")]
        protected void AutoNyaClicked()
        {
            AutoNyaButtonToggle = !AutoNyaActive;
            ToggleAutoNya(AutoNyaButtonToggle);
        }

        #endregion actions

        public virtual void Initialize()
        {
            ImageUtils.ErrorSpriteLoadedEvent += ImageUtilsOnErrorSpriteLoadedEvent;
        }
        
        public virtual void Dispose()
        {
            ImageUtils.ErrorSpriteLoadedEvent -= ImageUtilsOnErrorSpriteLoadedEvent;
        }

        private void ImageUtilsOnErrorSpriteLoadedEvent()
        {
            if (AutoNyaActive)
            {
                ToggleAutoNya(false);
            }
        }

        protected void ToggleAutoNya(bool active)
        {
            switch (active)
            {
                case true:
                    AutoNyaActive = true;
                    _tickableManager.AddLate(_autoNyaManager);
                
                    NyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = Color.green;
                    NyaButton.interactable = false;
                    break;
                case false:
                {
                    AutoNyaActive = false;
                    _tickableManager.RemoveLate(_autoNyaManager);

                    if (!_autoNyaManager.DoingDaThing)
                    {
                        NyaButton.interactable = true;
                    }

                    NyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f); // Beatgames why 0.502
                    break;
                }
            }
        }

        private class AutoNyaManager : ILateTickable
        {
            public bool DoingDaThing;
            private readonly ImageUtils _imageUtils;
            private readonly PluginConfig _pluginConfig;
            private static DateTime _lastNyaTime = DateTime.Now;
            private readonly NyaViewController _nyaViewController;

            public AutoNyaManager(ImageUtils imageUtils, PluginConfig pluginConfig, NyaViewController nyaViewController)
            {
                _imageUtils = imageUtils;
                _pluginConfig = pluginConfig;
                _nyaViewController = nyaViewController;
            }

            public void LateTick()
            {
                if (_lastNyaTime.TimeOfDay < DateTime.Now.TimeOfDay && !DoingDaThing)
                {
                    DoingDaThing = true;
                    _imageUtils.LoadNewNyaImage(_nyaViewController.NyaImage, () =>
                    {
                        _lastNyaTime = DateTime.Now.AddSeconds(_pluginConfig.AutoNyaWait);
                        DoingDaThing = false;

                        if (!AutoNyaActive)
                        {
                            _nyaViewController.NyaButton.interactable = true;
                        }
                    });
                }
            }
        }
    }
}