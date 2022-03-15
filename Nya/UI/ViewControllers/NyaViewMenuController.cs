using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.GameplaySetup;
using HMUI;
using Nya.Configuration;
using Nya.CatCore;
using Nya.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Object = UnityEngine.Object;

namespace Nya.UI.ViewControllers
{
    internal class NyaViewMenuController : NyaViewController, IInitializable, IDisposable
    {
        private readonly UIUtils _uiUtils;
        private readonly CatCoreInfo _catCoreInfo;
        private readonly GameplaySetupViewController _gameplaySetupViewController;
        private readonly SettingsModalMenuController _settingsModalMenuController;

        private FloatingScreen? _floatingScreen;

        public NyaViewMenuController(PluginConfig config, ImageUtils imageUtils, UIUtils uiUtils, CatCoreInfo catCoreInfo, GameplaySetupViewController gameplaySetupViewController, SettingsModalMenuController settingsModalMenuController)
            : base(config, imageUtils)
        {
            _uiUtils = uiUtils;
            _catCoreInfo = catCoreInfo;
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

            _catCoreInfo.CurrentImageView = NyaImage;
            // SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
        }

        private void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            
        }

        public void Dispose()
        {
            if (GameplaySetup.IsSingletonAvailable)
            {
                GameplaySetup.instance.RemoveTab("Nya");
            }

            if (Config.InMenu)
            {
                _floatingScreen!.HandleReleased -= FloatingScreen_HandleReleased;
                Object.Destroy(_floatingScreen);
            }

            _gameplaySetupViewController.didActivateEvent -= GameplaySetupViewController_didActivateEvent;
            _gameplaySetupViewController.didDeactivateEvent -= GameplaySetupViewController_didDeactivateEvent;
        }

        private void GameplaySetupViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                return;
            }

            if (Config.InMenu && (_floatingScreen!.transform.position != Config.MenuPosition || _floatingScreen.transform.rotation.eulerAngles != Config.MenuRotation)) // in case game floatingscreen got moved
            {
                _floatingScreen.transform.position = Config.MenuPosition;
                _floatingScreen.transform.rotation = Quaternion.Euler(Config.MenuRotation);
            }

            _catCoreInfo.CurrentImageView = NyaImage;
            NyaButton.interactable = false;
            ImageUtils.LoadNyaImage(NyaImage);
            NyaButton.interactable = true;
        }

        private void MenuDeactived()
        {
            if (AutoNyaToggle)
            {
                AutoNyaToggle = false;
                NyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f);
                NyaButton.interactable = true;
            }

            _catCoreInfo.CurrentImageView = null;
            _settingsModalMenuController.HideModal();
        }

        private void FloatingScreen_HandleReleased(object sender, FloatingScreenHandleEventArgs args)
        {
            var transform = _floatingScreen!.transform;
            Config.MenuPosition = transform.position;
            Config.MenuRotation = transform.eulerAngles;
        }

        public void ReloadFloatingScreenPosition()
        {
            var transform = _floatingScreen!.transform;
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

            _settingsModalMenuController.ShowModal(SettingsButtonTransform);
        }
    }
}