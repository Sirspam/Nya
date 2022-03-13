using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using Nya.Configuration;
using Nya.CatCore;
using Nya.Utils;
using Tweening;
using UnityEngine;
using Zenject;

namespace Nya.UI.ViewControllers
{
    internal class NyaViewGameController : NyaViewController, IInitializable, IDisposable
    {
        private readonly UIUtils _uiUtils;
        private readonly IGamePause _gamePause;
        private readonly CatCoreInfo _catCoreInfo;
        private readonly TimeTweeningManager _timeTweeningManager;
        private readonly SettingsModalGameController _settingsModalGameController;

        private FloatingScreen? _floatingScreen;

        public NyaViewGameController(PluginConfig pluginConfig, ImageUtils imageUtils, UIUtils uiUtils, IGamePause gamePause, CatCoreInfo catCoreInfo, TimeTweeningManager timeTweeningManager, SettingsModalGameController settingsModalGameController)
            : base(pluginConfig, imageUtils)
        {
            _uiUtils = uiUtils;
            _gamePause = gamePause;
            _catCoreInfo = catCoreInfo;
            _timeTweeningManager = timeTweeningManager;
            _settingsModalGameController = settingsModalGameController;
        }

        public void Initialize()
        {
            _floatingScreen = PluginConfig.SeparatePositions ? _uiUtils.CreateNyaFloatingScreen(this, PluginConfig.PausePosition, Quaternion.Euler(PluginConfig.PauseRotation)) : _uiUtils.CreateNyaFloatingScreen(this, PluginConfig.MenuPosition, Quaternion.Euler(PluginConfig.MenuRotation));
            _floatingScreen.gameObject.name = "NyaGameFloatingScreen";
            
            _floatingScreen.gameObject.SetActive(false);
            _floatingScreen.HandleReleased += FloatingScreen_HandleReleased;
            _gamePause.didPauseEvent += GamePause_didPauseEvent;
            _gamePause.willResumeEvent += GamePause_didResumeEvent;
        }

        public void Dispose()
        {
            _gamePause.didPauseEvent -= GamePause_didPauseEvent;
            _gamePause.willResumeEvent -= GamePause_didResumeEvent;
            _floatingScreen!.HandleReleased -= FloatingScreen_HandleReleased;
            _settingsModalGameController.HideModal();
            _floatingScreen.gameObject.SetActive(false);
            _timeTweeningManager.KillAllTweens(_floatingScreen);
        }

        private void GamePause_didPauseEvent()
        {
            _catCoreInfo.CurrentImageView = NyaImage;
            _floatingScreen!.gameObject.SetActive(true);
        }

        private void GamePause_didResumeEvent()
        {
            if (AutoNyaToggle)
            {
                AutoNyaToggle = false;
                NyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f);
                NyaButton.interactable = true;
            }
            _catCoreInfo.CurrentImageView = null;
            _settingsModalGameController.HideModal();
            _floatingScreen!.gameObject.SetActive(false);
        }

        private void FloatingScreen_HandleReleased(object sender, FloatingScreenHandleEventArgs args)
        {
            var transform = _floatingScreen!.transform;

            if (PluginConfig.SeparatePositions)
            {
                PluginConfig.PausePosition = transform.position;
                PluginConfig.PauseRotation = transform.eulerAngles;
            }
            else
            {
                PluginConfig.MenuPosition = transform.position;
                PluginConfig.MenuRotation = transform.eulerAngles;
            }
        }

        [UIAction("settings-button-clicked")]
        protected void SettingsButtonClicked()
        {
            if (AutoNyaToggle)
            {
                AutoNya();
            }
            _settingsModalGameController.ShowModal(SettingsButtonTransform);
        }
    }
}