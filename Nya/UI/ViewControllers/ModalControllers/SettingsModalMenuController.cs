using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using Nya.Configuration;
using Nya.Managers;
using Nya.UI.FlowCoordinators;
using Nya.UI.ViewControllers.SettingsControllers;
using Nya.Utils;
using Tweening;
using UnityEngine;

namespace Nya.UI.ViewControllers.ModalControllers
{
    internal class SettingsModalMenuController : SettingsModalController
    {
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly NyaSettingsFlowCoordinator _nyaSettingsFlowCoordinator;
        private readonly NyaSettingsMainViewController _nyaSettingsMainViewController;

        public SettingsModalMenuController(UIUtils uiUtils, ImageUtils imageUtils, MainCamera mainCamera, PluginConfig pluginConfig, NyaImageManager nyaImageManager, FloatingScreenUtils floatingScreenUtils, ImageSourcesManager imageSourcesManager, TimeTweeningManager timeTweeningManager, NsfwConfirmModalController nsfwConfirmModalController, MainFlowCoordinator mainFlowCoordinator, NyaSettingsFlowCoordinator nyaSettingsFlowCoordinator, NyaSettingsMainViewController nyaSettingsMainViewController)
            : base(uiUtils, imageUtils, mainCamera, pluginConfig, nyaImageManager, floatingScreenUtils, imageSourcesManager, timeTweeningManager, nsfwConfirmModalController)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _nyaSettingsFlowCoordinator = nyaSettingsFlowCoordinator;
            _nyaSettingsMainViewController = nyaSettingsMainViewController;
        }

        public void ShowModal(Transform parentTransform)
        {
            ShowModal(parentTransform, this);
            if (!PluginConfig.InMenu)
            {
                ScreenTab.IsVisible = false;
            }
        }
        
        [UIValue("load-position-button-text")]
        protected override string LoadPositionButtonText => PluginConfig.SeparatePositions ? "Load Menu Position" : "Load Position";

        [UIAction("show-nya-settings")]
        private void ShowNyaSettings()
        {
            ModalView.HandleBlockerButtonClicked();
            if (_nyaSettingsMainViewController.isActiveAndEnabled)
            {
                return;
            }

            _nyaSettingsMainViewController.parentFlowCoordinator = _mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
            _nyaSettingsMainViewController.parentFlowCoordinator.PresentFlowCoordinator(_nyaSettingsFlowCoordinator, animationDirection: ViewController.AnimationDirection.Vertical);
        }
    }
}