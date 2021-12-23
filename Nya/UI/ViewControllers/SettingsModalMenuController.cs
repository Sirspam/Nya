using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using UnityEngine;

namespace Nya.UI.ViewControllers
{
    public class SettingsModalMenuController : SettingsModalController
    {
        private readonly MainFlowCoordinator mainFlowCoordinator;
        private readonly NyaSettingsFlowCoordinator nyaSettingsFlowCoordinator;
        private readonly SettingsViewMainPanelController settingsViewMainPanelController;

        public SettingsModalMenuController(MainCamera mainCamera, NsfwConfirmModalController nsfwConfirmModalController, UIUtils uiUtils, MainFlowCoordinator mainFlowCoordinator, NyaSettingsFlowCoordinator nyaSettingsFlowCoordinator, SettingsViewMainPanelController settingsViewMainPanelController) : base(mainCamera, nsfwConfirmModalController, uiUtils)
        {
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.nyaSettingsFlowCoordinator = nyaSettingsFlowCoordinator;
            this.settingsViewMainPanelController = settingsViewMainPanelController;
        }

        public void ShowModal(Transform parentTransform)
        {
            ShowModal(parentTransform, this);
            if (!PluginConfig.Instance.InMenu) ScreenTab.IsVisible = false;
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