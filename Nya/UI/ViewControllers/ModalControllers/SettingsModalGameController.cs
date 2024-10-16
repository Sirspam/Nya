using BeatSaberMarkupLanguage.Attributes;
using Nya.Configuration;
using Nya.Managers;
using Nya.Utils;
using Tweening;
using UnityEngine;

namespace Nya.UI.ViewControllers.ModalControllers
{
    internal class SettingsModalGameController : SettingsModalController
    {
        public SettingsModalGameController(UIUtils uiUtils, ImageUtils imageUtils, MainCamera mainCamera, PluginConfig pluginConfig, NyaImageManager nyaImageManager, FloatingScreenUtils floatingScreenUtils, ImageSourcesManager imageSourcesManager, TimeTweeningManager timeTweeningManager, NsfwConfirmModalController nsfwConfirmModalController)
            : base(uiUtils, imageUtils, mainCamera, pluginConfig, nyaImageManager, floatingScreenUtils, imageSourcesManager, timeTweeningManager, nsfwConfirmModalController)
        {
        }

        public void ShowModal(Transform parentTransform)
        {
            ShowModal(parentTransform, this);
            MoreSettingsTab.IsVisible = false;
        }

        [UIValue("load-position-button-text")]
        protected override string LoadPositionButtonText => PluginConfig.SeparatePositions ? "Load Game Position" : "Load Position";
    }
}