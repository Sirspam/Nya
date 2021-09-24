using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.GameplaySetup;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using System;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace Nya.UI.ViewControllers
{
    internal class NyaViewMenuController : NyaViewController, IInitializable, IDisposable
    {
        private readonly SettingsModalController settingsModalController;
        private readonly GameplaySetupViewController gameplaySetupViewController;
        private FloatingScreen floatingScreen;


        public NyaViewMenuController(SettingsModalController settingsModalController, GameplaySetupViewController gameplaySetupViewController) : base(settingsModalController)
        {
            this.settingsModalController = settingsModalController;
            this.gameplaySetupViewController = gameplaySetupViewController;
        }
        public void Initialize()
        {
            if (PluginConfig.Instance.inMenu)
            {
                floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100f, 80f), PluginConfig.Instance.showHandle, PluginConfig.Instance.menuPosition, Quaternion.Euler(PluginConfig.Instance.menuRotation), curvatureRadius: 220, true);
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.NyaView.bsml"), floatingScreen.gameObject, this);
                if (PluginConfig.Instance.showHandle)
                {
                    floatingScreen.handle.transform.localPosition = new Vector3(0f, -floatingScreen.ScreenSize.y / 1.8f, -5f);
                    floatingScreen.handle.transform.localScale = new Vector3(floatingScreen.ScreenSize.x * 0.8f, floatingScreen.ScreenSize.y / 15f, floatingScreen.ScreenSize.y / 15f);
                    floatingScreen.HandleReleased += FloatingScreen_HandleReleased;
                }
                floatingScreen.gameObject.name = "NyaMenuFloatingScreen";
                floatingScreen.gameObject.layer = 5;
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

        private void FloatingScreen_HandleReleased(object sender, FloatingScreenHandleEventArgs args)
        {
            PluginConfig.Instance.menuPosition = floatingScreen.transform.position;
            PluginConfig.Instance.menuRotation = floatingScreen.transform.eulerAngles;
        }
    }
}
