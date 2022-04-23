using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using Nya.Configuration;
using Nya.UI.ViewControllers.ModalControllers;
using Nya.Utils;
using Tweening;
using UnityEngine;
using Zenject;

namespace Nya.UI.ViewControllers.NyaViewControllers
{
    internal class NyaViewGameController : NyaViewController, IInitializable, IDisposable
    {
        private readonly IGamePause _gamePause;
        private readonly FloatingScreenUtils _floatingScreenUtils;
        private readonly TimeTweeningManager _timeTweeningManager;
        private readonly SettingsModalGameController _settingsModalGameController;
        
        public NyaViewGameController(PluginConfig pluginConfig, ImageUtils imageUtils, IGamePause gamePause, FloatingScreenUtils floatingScreenUtils, TimeTweeningManager timeTweeningManager, SettingsModalGameController settingsModalGameController)
            : base(pluginConfig, imageUtils)
        {
            _gamePause = gamePause;
            _floatingScreenUtils = floatingScreenUtils;
            _timeTweeningManager = timeTweeningManager;
            _settingsModalGameController = settingsModalGameController;
        }

        public void Initialize()
        {
            if (_floatingScreenUtils.GameFloatingScreen == null)
            {
                _floatingScreenUtils.CreateNyaFloatingScreen(this, FloatingScreenUtils.FloatingScreenType.Game);
            }

            _floatingScreenUtils.GameFloatingScreen!.gameObject.SetActive(false);
            _floatingScreenUtils.GameFloatingScreen!.HandleGrabbed += FloatingScreen_HandleReleased;

            _gamePause.didPauseEvent += GamePause_didPauseEvent;
            _gamePause.willResumeEvent += GamePause_didResumeEvent;
        }

        public void Dispose()
        {
            _gamePause.didPauseEvent -= GamePause_didPauseEvent;
            _gamePause.willResumeEvent -= GamePause_didResumeEvent;
            _floatingScreenUtils.GameFloatingScreen!.HandleReleased -= FloatingScreen_HandleReleased;
            _settingsModalGameController.HideModal();
            _floatingScreenUtils.GameFloatingScreen.gameObject.SetActive(false);
            _timeTweeningManager.KillAllTweens(_floatingScreenUtils.GameFloatingScreen);
        }

        private void GamePause_didPauseEvent()
        {
            _floatingScreenUtils.GameFloatingScreen!.gameObject.SetActive(true);
        }

        private void GamePause_didResumeEvent()
        {
            if (AutoNyaToggle)
            {
                AutoNyaToggle = false;
                NyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f);
                NyaButton.interactable = true;
            }
            _settingsModalGameController.HideModal();
            _floatingScreenUtils.GameFloatingScreen!.gameObject.SetActive(false);
        }

        private void FloatingScreen_HandleReleased(object sender, FloatingScreenHandleEventArgs args)
        {
            var transform = _floatingScreenUtils.GameFloatingScreen!.transform;

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