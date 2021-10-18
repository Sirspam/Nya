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
        private readonly SettingsModalController settingsModalController;
        private readonly GameplaySetupViewController gameplaySetupViewController;
        private readonly UIUtils _uiUtils;
        private FloatingScreen floatingScreen;


        public NyaViewMenuController(SettingsModalController settingsModalController, UIUtils uiUtils, GameplaySetupViewController gameplaySetupViewController) : base(settingsModalController)
        {
            this.settingsModalController = settingsModalController;
            this.gameplaySetupViewController = gameplaySetupViewController;
            _uiUtils = uiUtils;
        }

        public void Initialize()
        {
            if (PluginConfig.Instance.inMenu)
            {
                floatingScreen = _uiUtils.CreateNyaFloatingScreen(this, PluginConfig.Instance.menuPosition, Quaternion.Euler(PluginConfig.Instance.menuRotation));
                floatingScreen.gameObject.name = "NyaMenuFloatingScreen";
            }
            else
            {
                GameplaySetup.instance.AddTab("Nya", "Nya.UI.Views.NyaView.bsml", this);
            }
            gameplaySetupViewController.didActivateEvent += GameplaySetupViewController_didActivateEvent;
            gameplaySetupViewController.didDeactivateEvent += GameplaySetupViewController_didDeactivateEvent;
        }

        public void Dispose()
        {
            if (GameplaySetup.IsSingletonAvailable)
            {
                GameplaySetup.instance.RemoveTab("Nya");
            }

            gameplaySetupViewController.didActivateEvent -= GameplaySetupViewController_didActivateEvent;
            gameplaySetupViewController.didDeactivateEvent -= GameplaySetupViewController_didDeactivateEvent;
        }

        private async void GameplaySetupViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (!firstActivation)
            {
                nyaButton.interactable = false;
                await ImageUtils.LoadNyaSprite(nyaImage);
                nyaButton.interactable = true;
            }
        }

        private void GameplaySetupViewController_didDeactivateEvent(bool firstActivation, bool addedToHierarchy)
        {
            if (autoNyaToggle)
            {
                autoNyaToggle = false;
                nyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f);
                nyaButton.interactable = true;
            }
        }
    }
}
