using Nya.Utils;
using Nya.Configuration;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using System;
using System.Threading.Tasks;
using Zenject;
using HMUI;
using UnityEngine;
using TMPro;

namespace Nya.UI.ViewControllers
{
    public class NyaModifierController : IInitializable, IDisposable
    {
        private SettingsModalController settingsModalController;
        private GameplaySetupViewController gameplaySetupViewController;
        private bool autoNyaToggle = false;

        #region components
        [UIComponent("nyaImage")]
        internal readonly ImageView nyaImage;

        [UIComponent("nyaButton")]
        private readonly UnityEngine.UI.Button nyaButton;

        [UIComponent("nyaDownloadButton")]
        private readonly UnityEngine.UI.Button nyaDownloadButton;

        [UIComponent("nyaAutoButton")]
        private readonly TextMeshProUGUI nyaAutoButton;

        [UIComponent("settingsButton")]
        private readonly RectTransform settingsButtonTransform;
        #endregion

        #region actions
        [UIAction("nya-click")]
        public async void NyaClicked()
        {
            nyaButton.interactable = false;
            await ImageUtils.loadNewNyaSprite(nyaImage);
            nyaButton.interactable = true;
        }

        [UIAction("#post-parse")]
        public async void NyaPostParse()
        {
            await ImageUtils.loadNewNyaSprite(nyaImage);
        }

        [UIAction("nya-auto-clicked")]
        public async void autoNya()
        {
            autoNyaToggle = !autoNyaToggle;
            if (autoNyaToggle) // On
            {
                nyaAutoButton.faceColor = new Color32(46, 204, 113, 255); // Green
                nyaButton.interactable = false;
                while (autoNyaToggle)
                {
                    await ImageUtils.loadNewNyaSprite(nyaImage);
                    await Task.Delay(PluginConfig.Instance.autoNyaWait * 1000);
                }
            }
            else // Off
            {
                nyaAutoButton.faceColor = new Color32(255, 255, 255, 255); // White
                nyaButton.interactable = true;
            }
        }

        [UIAction("settings-button-clicked")]
        public async void SettingsButtonClicked()
        {
            if (autoNyaToggle)
            {
                autoNya();
            }
            settingsModalController.ShowModal(settingsButtonTransform);
        }
        #endregion

        public NyaModifierController(SettingsModalController settingsModalController, GameplaySetupViewController gameplaySetupViewController)
        {
            this.settingsModalController = settingsModalController;
            this.gameplaySetupViewController = gameplaySetupViewController;
        }

        private void GameplaySetupViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy)
        {
            if (autoNyaToggle)
            {
                autoNyaToggle = false;
                nyaAutoButton.faceColor = new Color32(255, 255, 255, 255); // White
                nyaButton.interactable = true;
            }
        }
        public void Initialize()
        {
            GameplaySetup.instance.AddTab("Nya", "Nya.UI.Views.NyaModifierView.bsml", this);
            gameplaySetupViewController.didDeactivateEvent += GameplaySetupViewController_didActivateEvent;
        }
        public void Dispose()
        {
            if(GameplaySetup.instance!=null)
            {
                GameplaySetup.instance.RemoveTab("Nya");
            }
            gameplaySetupViewController.didDeactivateEvent -= GameplaySetupViewController_didActivateEvent;
        }
    }
}
