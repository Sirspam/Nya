using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using UnityEngine;

namespace Nya.UI.ViewControllers
{
    internal class SettingsModalMenuController : SettingsModalController
    {
        private readonly MainFlowCoordinator mainFlowCoordinator;
        private readonly NyaSettingsFlowCoordinator nyaSettingsFlowCoordinator;
        private readonly SettingsViewMainPanelController settingsViewMainPanelController;

        public SettingsModalMenuController(PluginConfig config, ImageUtils imageUtils, UIUtils uiUtils, MainCamera mainCamera, NsfwConfirmModalController nsfwConfirmModalController,  MainFlowCoordinator mainFlowCoordinator, NyaSettingsFlowCoordinator nyaSettingsFlowCoordinator, SettingsViewMainPanelController settingsViewMainPanelController)
            : base(config,imageUtils, uiUtils, nsfwConfirmModalController, mainCamera)
        {
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.nyaSettingsFlowCoordinator = nyaSettingsFlowCoordinator;
            this.settingsViewMainPanelController = settingsViewMainPanelController;
        }

        public void ShowModal(Transform parentTransform)
        {
            ShowModal(parentTransform, this);
            if (!Config.InMenu) ScreenTab.IsVisible = false;
        }

        [UIAction("show-nya-settings")]
        private void ShowNyaSettings()
        {
            HideModal();
            if (settingsViewMainPanelController.isActiveAndEnabled) return;
            settingsViewMainPanelController.parentFlowCoordinator = mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
            settingsViewMainPanelController.parentFlowCoordinator.PresentFlowCoordinator(nyaSettingsFlowCoordinator, animationDirection: ViewController.AnimationDirection.Vertical);
        }
    }
}