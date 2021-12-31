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
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly NyaSettingsFlowCoordinator _nyaSettingsFlowCoordinator;
        private readonly SettingsViewMainPanelController _settingsViewMainPanelController;

        public SettingsModalMenuController(PluginConfig config, ImageUtils imageUtils, UIUtils uiUtils, MainCamera mainCamera, NsfwConfirmModalController nsfwConfirmModalController,  MainFlowCoordinator mainFlowCoordinator, NyaSettingsFlowCoordinator nyaSettingsFlowCoordinator, SettingsViewMainPanelController settingsViewMainPanelController)
            : base(config,imageUtils, uiUtils, nsfwConfirmModalController, mainCamera)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _nyaSettingsFlowCoordinator = nyaSettingsFlowCoordinator;
            _settingsViewMainPanelController = settingsViewMainPanelController;
        }

        public void ShowModal(Transform parentTransform)
        {
            ShowModal(parentTransform, this);
            if (!Config.InMenu)
            {
                ScreenTab.IsVisible = false;
            }
        }

        [UIAction("show-nya-settings")]
        private void ShowNyaSettings()
        {
            HideModal();
            if (_settingsViewMainPanelController.isActiveAndEnabled)
            {
                return;
            }

            _settingsViewMainPanelController.parentFlowCoordinator = _mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
            _settingsViewMainPanelController.parentFlowCoordinator.PresentFlowCoordinator(_nyaSettingsFlowCoordinator, animationDirection: ViewController.AnimationDirection.Vertical);
        }
    }
}