using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using Nya.Configuration;
using Nya.UI.ViewControllers.ModalControllers;
using Nya.Utils;
using Tweening;
using Zenject;

namespace Nya.UI.ViewControllers.NyaViewControllers
{
    internal class NyaViewGameController : NyaViewController, IInitializable, IDisposable
    {
        private readonly IGamePause _gamePause;
        private readonly FloatingScreenUtils _floatingScreenUtils;
        private readonly TimeTweeningManager _timeTweeningManager;
        private readonly SettingsModalGameController _settingsModalGameController;
        
        public NyaViewGameController(ImageUtils imageUtils, PluginConfig pluginConfig, TickableManager tickableManager, IGamePause gamePause, FloatingScreenUtils floatingScreenUtils, TimeTweeningManager timeTweeningManager, SettingsModalGameController settingsModalGameController)
            : base(imageUtils, pluginConfig, tickableManager)
        {
            _gamePause = gamePause;
            _floatingScreenUtils = floatingScreenUtils;
            _timeTweeningManager = timeTweeningManager;
            _settingsModalGameController = settingsModalGameController;
        }

        public override void Initialize()
        {
            base.Initialize();
            
            if (_floatingScreenUtils.GameFloatingScreen == null)
            {
                _floatingScreenUtils.CreateNyaFloatingScreen(this, FloatingScreenUtils.FloatingScreenType.Game);
            }

            _floatingScreenUtils.GameFloatingScreen!.gameObject.SetActive(false);
            _floatingScreenUtils.GameFloatingScreen!.HandleReleased += FloatingScreen_HandleReleased;

            _gamePause.didPauseEvent += GamePause_didPauseEvent;
            _gamePause.willResumeEvent += GamePause_didResumeEvent;
        }

        public override void Dispose()
        {
            base.Dispose();
            
            if (AutoNyaActive)
            {
                ToggleAutoNya(false);
            }
            
            _settingsModalGameController.HideModal();
            _timeTweeningManager.KillAllTweens(_floatingScreenUtils.GameFloatingScreen);
            
            _gamePause.didPauseEvent -= GamePause_didPauseEvent;
            _gamePause.willResumeEvent -= GamePause_didResumeEvent;
            _floatingScreenUtils.GameFloatingScreen!.HandleReleased -= FloatingScreen_HandleReleased;

            if (_settingsModalGameController.ModalView != null)
            {
                _settingsModalGameController.ModalView.blockerClickedEvent -= ModalViewOnBlockerClickedEvent;
            }
        }

        private void GamePause_didPauseEvent()
        {
            _floatingScreenUtils.GameFloatingScreen!.gameObject.SetActive(true);
            
            if (PluginConfig.PersistantAutoNya && AutoNyaButtonToggle && !AutoNyaActive)
            {
                ToggleAutoNya(true);   
            }
        }

        private void GamePause_didResumeEvent()
        {
            if (AutoNyaActive)
            {
                ToggleAutoNya(false);
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
        
        private void ModalViewOnBlockerClickedEvent()
        {
            if (PluginConfig.PersistantAutoNya && AutoNyaButtonToggle && !AutoNyaActive)
            {
                ToggleAutoNya(true);
            }
        }

        [UIAction("settings-button-clicked")]
        protected void SettingsButtonClicked()
        {
            if (AutoNyaActive)
            {
                ToggleAutoNya(false);
            }
            
            _settingsModalGameController.ShowModal(SettingsButtonTransform);
            _settingsModalGameController.ModalView.blockerClickedEvent += ModalViewOnBlockerClickedEvent;
        }
    }
}