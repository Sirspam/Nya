using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using Nya.Configuration;
using Nya.UI.ViewControllers.ModalControllers;
using Nya.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Object = UnityEngine.Object;

namespace Nya.UI.ViewControllers.NyaViewControllers
{
    internal class NyaViewMenuController : NyaViewController, IInitializable, IDisposable
    {
        private readonly GameScenesManager _gameScenesManager;
        private readonly FloatingScreenUtils _floatingScreenUtils;
        private readonly SettingsModalMenuController _settingsModalMenuController;
        
        public NyaViewMenuController(ImageUtils imageUtils, PluginConfig pluginConfig,  TickableManager tickableManager, GameScenesManager gameScenesManager, FloatingScreenUtils floatingScreenUtils, SettingsModalMenuController settingsModalMenuController)
            : base(imageUtils, pluginConfig, tickableManager)
        {
            _gameScenesManager = gameScenesManager;
            _floatingScreenUtils = floatingScreenUtils;
            _settingsModalMenuController = settingsModalMenuController;
        }
        
        public override void Initialize()
        {
            base.Initialize();

            if (_floatingScreenUtils.MenuFloatingScreen == null)
            {
                _floatingScreenUtils.CreateNyaFloatingScreen(this, FloatingScreenUtils.FloatingScreenType.Menu);
            }

            _floatingScreenUtils.MenuFloatingScreen!.HandleReleased += FloatingScreen_HandleReleased;
            
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
        }

        public override void Dispose()
        {
            base.Dispose();
            
            if (_floatingScreenUtils.MenuFloatingScreen != null)
            {
                _floatingScreenUtils.MenuFloatingScreen!.HandleReleased -= FloatingScreen_HandleReleased;
                Object.Destroy(_floatingScreenUtils.MenuFloatingScreen);
            }

            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
            if (_settingsModalMenuController.ModalView != null)
            {
                _settingsModalMenuController.ModalView.blockerClickedEvent -= ModalViewOnBlockerClickedEvent;
            }
        }

        private void ModalViewOnBlockerClickedEvent()
        {
            if (PluginConfig.PersistantAutoNya && AutoNyaButtonToggle && !AutoNyaActive)
            {
                ToggleAutoNya(true);
            }
        }

        private void SceneManagerOnActiveSceneChanged(Scene currentScene, Scene nextScene)
        {
            _settingsModalMenuController.HideModal();
            
            if (nextScene.name == "MainMenu")
            {
                _gameScenesManager.transitionDidFinishEvent -= MenuActivated;
                _gameScenesManager.transitionDidFinishEvent += MenuActivated;
            }
            else
            {
                _gameScenesManager.transitionDidFinishEvent -= MenuDeactivated;
                _gameScenesManager.transitionDidFinishEvent += MenuDeactivated;
            }
        }

        private void MenuActivated(ScenesTransitionSetupDataSO transitionSetupData, DiContainer diContainer)
        {
            _gameScenesManager.transitionDidFinishEvent -= MenuActivated;
            
            if (_floatingScreenUtils.MenuFloatingScreen != null &&
                _floatingScreenUtils.MenuFloatingScreen.isActiveAndEnabled &&
                (_floatingScreenUtils.MenuFloatingScreen!.transform.position != PluginConfig.MenuPosition ||
                 _floatingScreenUtils.MenuFloatingScreen.transform.rotation.eulerAngles != PluginConfig.MenuRotation))
            {
                _floatingScreenUtils.MenuFloatingScreen.transform.position = PluginConfig.MenuPosition;
                _floatingScreenUtils.MenuFloatingScreen.transform.rotation = Quaternion.Euler(PluginConfig.MenuRotation);
            }

            NyaButton.interactable = false;
            ImageUtils.LoadCurrentNyaImage(NyaImage, () =>
            {
                NyaButton.interactable = true;
                if (PluginConfig.PersistantAutoNya && AutoNyaButtonToggle && !AutoNyaActive)
                {
                    ToggleAutoNya(true);
                }
            });
        }
        
        private void MenuDeactivated(ScenesTransitionSetupDataSO transitionSetupData, DiContainer diContainer)
        {
            _gameScenesManager.transitionDidFinishEvent -= MenuDeactivated;
            
            if (AutoNyaActive)
            {
                ToggleAutoNya(false);
            }
            
            _settingsModalMenuController.HideModal();
        }

        private void FloatingScreen_HandleReleased(object sender, FloatingScreenHandleEventArgs args)
        {
            var transform = _floatingScreenUtils.MenuFloatingScreen!.transform;
            PluginConfig.MenuPosition = transform.position;
            PluginConfig.MenuRotation = transform.eulerAngles;
        }

        [UIAction("settings-button-clicked")]
        protected void SettingsButtonClicked()
        {
            if (AutoNyaActive)
            {
                ToggleAutoNya(false);
            }
            
            _settingsModalMenuController.ShowModal(SettingsButtonTransform);
            _settingsModalMenuController.ModalView.blockerClickedEvent += ModalViewOnBlockerClickedEvent;
        }
    }
}