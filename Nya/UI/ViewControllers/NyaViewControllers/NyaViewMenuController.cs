using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.GameplaySetup;
using HMUI;
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
        private readonly UIUtils _uiUtils;
        private readonly FloatingScreenUtils _floatingScreenUtils;
        private readonly GameplaySetupViewController _gameplaySetupViewController;
        private readonly SettingsModalMenuController _settingsModalMenuController;
        
        public NyaViewMenuController(PluginConfig pluginConfig, ImageUtils imageUtils, UIUtils uiUtils, FloatingScreenUtils floatingScreenUtils, GameplaySetupViewController gameplaySetupViewController, SettingsModalMenuController settingsModalMenuController)
            : base(pluginConfig, imageUtils)
        {
            _uiUtils = uiUtils;
            _floatingScreenUtils = floatingScreenUtils;
            _gameplaySetupViewController = gameplaySetupViewController;
            _settingsModalMenuController = settingsModalMenuController;
        }

        public void Initialize()
        {
            if (PluginConfig.InMenu)
            {
                if (_floatingScreenUtils.MenuFloatingScreen == null)
                {
                    _floatingScreenUtils.CreateNyaFloatingScreen(this, FloatingScreenUtils.FloatingScreenType.Menu);
                }

                _floatingScreenUtils.MenuFloatingScreen!.HandleGrabbed += FloatingScreen_HandleReleased;
            }
            else
            {
                GameplaySetup.instance.AddTab("Nya", "Nya.UI.Views.NyaView.bsml", this);
            }
            
            SceneManager.activeSceneChanged += SceneManagerOnactiveSceneChanged;
        }

        private void SceneManagerOnactiveSceneChanged(Scene currentScene, Scene nextScene)
        {
            if (nextScene.name == "MainMenu")
            {
                MenuActivated();
            }
            else
            {
                MenuDeactivated();
            }
        }

        public void Dispose()
        {
            if (GameplaySetup.IsSingletonAvailable)
            {
                GameplaySetup.instance.RemoveTab("Nya");
            }

            if (_floatingScreenUtils.MenuFloatingScreen != null)
            {
                _floatingScreenUtils.MenuFloatingScreen!.HandleReleased -= FloatingScreen_HandleReleased;
                Object.Destroy(_floatingScreenUtils.MenuFloatingScreen);
            }

            _gameplaySetupViewController.didActivateEvent -= GameplaySetupViewController_didActivateEvent;
        }

        private void GameplaySetupViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (!firstActivation)
            {
                MenuActivated();
            }
        }

        private void MenuActivated()
        {
            // in case game Game floating screen got moved
            if (PluginConfig.InMenu && (_floatingScreenUtils.MenuFloatingScreen!.transform.position != PluginConfig.MenuPosition || _floatingScreenUtils.MenuFloatingScreen.transform.rotation.eulerAngles != PluginConfig.MenuRotation))
            {
                _floatingScreenUtils.MenuFloatingScreen.transform.position = PluginConfig.MenuPosition;
                _floatingScreenUtils.MenuFloatingScreen.transform.rotation = Quaternion.Euler(PluginConfig.MenuRotation);
            }
            
            NyaButton.interactable = false;
            ImageUtils.LoadCurrentNyaImage(NyaImage, () =>
            {
                NyaButton.interactable = true;
                if (ImageUtils.AutoNyaActive)
                {
                    AutoNyaToggle = true;
                    AutoNya();
                }
            });
        }
        
        private void MenuDeactivated()
        {
            if (AutoNyaToggle)
            {
                AutoNyaToggle = false;
                NyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f);
                NyaButton.interactable = true;
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
            if (AutoNyaToggle)
            {
                AutoNya();
            }

            _settingsModalMenuController.ShowModal(SettingsButtonTransform);
        }
    }
}