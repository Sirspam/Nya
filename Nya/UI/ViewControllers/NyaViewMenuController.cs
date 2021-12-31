using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.GameplaySetup;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using System;
using UnityEngine;
using Zenject;

namespace Nya.UI.ViewControllers
{
    internal class NyaViewMenuController : NyaViewController, IInitializable, IDisposable
    {
        private readonly GameplaySetupViewController _gameplaySetupViewController;
        private readonly SettingsModalMenuController _settingsModalMenuController;
        private readonly UIUtils _uiUtils;

        private FloatingScreen _floatingScreen;

        public NyaViewMenuController(PluginConfig config, ImageUtils imageUtils, UIUtils uiUtils, GameplaySetupViewController gameplaySetupViewController, SettingsModalMenuController settingsModalMenuController)
            : base(config, imageUtils)
        {
            _uiUtils = uiUtils;
            _gameplaySetupViewController = gameplaySetupViewController;
            _settingsModalMenuController = settingsModalMenuController;
        }

        public void Initialize()
        {
            if (Config.InMenu)
            {
                _floatingScreen = _uiUtils.CreateNyaFloatingScreen(this, Config.MenuPosition, Quaternion.Euler(Config.MenuRotation));
                _floatingScreen.gameObject.name = "NyaMenuFloatingScreen";
                _floatingScreen.HandleReleased += FloatingScreen_HandleReleased;
            }
            else
            {
                GameplaySetup.instance.AddTab("Nya", "Nya.UI.Views.NyaView.bsml", this);
            }

            _gameplaySetupViewController.didActivateEvent += GameplaySetupViewController_didActivateEvent;
            _gameplaySetupViewController.didDeactivateEvent += GameplaySetupViewController_didDeactivateEvent;
        }

        public void Dispose()
        {
            if (GameplaySetup.IsSingletonAvailable)
            {
                GameplaySetup.instance.RemoveTab("Nya");
            }

            if (Config.InMenu)
            {
                _floatingScreen.HandleReleased -= FloatingScreen_HandleReleased;
            }

            _gameplaySetupViewController.didActivateEvent -= GameplaySetupViewController_didActivateEvent;
            _gameplaySetupViewController.didDeactivateEvent -= GameplaySetupViewController_didDeactivateEvent;
        }

        private void GameplaySetupViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy,
            bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                return;
            }

            if (Config.InMenu && (_floatingScreen.transform.position != Config.MenuPosition || _floatingScreen.transform.rotation.eulerAngles != Config.MenuRotation)) // in case game floatingscreen got moved
            {
                _floatingScreen.transform.position = Config.MenuPosition;
                _floatingScreen.transform.rotation = Quaternion.Euler(Config.MenuRotation);
            }

            nyaButton.interactable = false;
            ImageUtils.LoadNyaImage(nyaImage);
            nyaButton.interactable = true;
        }

        private void GameplaySetupViewController_didDeactivateEvent(bool firstActivation, bool addedToHierarchy)
        {
            if (AutoNyaToggle)
            {
                AutoNyaToggle = false;
                nyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f);
                nyaButton.interactable = true;
            }

            _settingsModalMenuController.HideModal();
        }

        private void FloatingScreen_HandleReleased(object sender, FloatingScreenHandleEventArgs args)
        {
            var transform = _floatingScreen.transform;
            Config.MenuPosition = transform.position;
            Config.MenuRotation = transform.eulerAngles;
        }

        public void ReloadFloatingScreenPosition()
        {
            var transform = _floatingScreen.transform;
            transform.position = Config.MenuPosition;
            transform.eulerAngles = Config.MenuRotation;
        }

        [UIAction("settings-button-clicked")]
        protected void SettingsButtonClicked()
        {
            if (AutoNyaToggle)
            {
                AutoNya();
            }

            _settingsModalMenuController.ShowModal(settingsButtonTransform);
        }
    }
}