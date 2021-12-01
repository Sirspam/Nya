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
    public class NyaViewMenuController : NyaViewController, IInitializable, IDisposable
    {
        private readonly GameplaySetupViewController gameplaySetupViewController;
        private readonly SettingsModalMenuController settingsModalMenuController;
        private readonly UIUtils uiUtils;
        public FloatingScreen floatingScreen;

        public NyaViewMenuController(GameplaySetupViewController gameplaySetupViewController, SettingsModalMenuController settingsModalMenuController, UIUtils uiUtils)
        {
            this.gameplaySetupViewController = gameplaySetupViewController;
            this.settingsModalMenuController = settingsModalMenuController;
            this.uiUtils = uiUtils;
        }

        public void Initialize()
        {
            
            
            if (PluginConfig.Instance.InMenu)
            {
                floatingScreen = uiUtils.CreateNyaFloatingScreen(this, PluginConfig.Instance.MenuPosition, Quaternion.Euler(PluginConfig.Instance.MenuRotation));
                floatingScreen.gameObject.name = "NyaMenuFloatingScreen";
                floatingScreen.HandleReleased += FloatingScreen_HandleReleased;
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
            floatingScreen.HandleReleased -= FloatingScreen_HandleReleased;
        }

        private async void GameplaySetupViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (!firstActivation)
            {
                if (floatingScreen.transform.position != PluginConfig.Instance.MenuPosition) // in case game floatingscreen got moved
                {
                    floatingScreen.transform.position = PluginConfig.Instance.MenuPosition;
                    floatingScreen.transform.rotation = Quaternion.Euler(PluginConfig.Instance.MenuRotation);
                }
                
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
            settingsModalMenuController.HideModal();
        }

        private void FloatingScreen_HandleReleased(object sender, FloatingScreenHandleEventArgs args)
        {
            PluginConfig.Instance.MenuPosition = floatingScreen.transform.position;
            PluginConfig.Instance.MenuRotation = floatingScreen.transform.eulerAngles;
        }

        public void ReloadFloatingScreenPosition()
        {
            floatingScreen.transform.position = PluginConfig.Instance.MenuPosition;
            floatingScreen.transform.eulerAngles = PluginConfig.Instance.MenuRotation;
        }

        [UIAction("settings-button-clicked")]
        protected void SettingsButtonClicked()
        {
            Plugin.Log.Debug("haha hi 1");
            
            if (autoNyaToggle)
            {
                AutoNya();
            }
            settingsModalMenuController.ShowModal(settingsButtonTransform);
        }
    }
}