using Nya.Configuration;
using Nya.Utils;
using Tweening;
using UnityEngine;

namespace Nya.UI.ViewControllers.ModalControllers
{
    internal class SettingsModalGameController : SettingsModalController
    {
        public SettingsModalGameController(UIUtils uiUtils, ImageUtils imageUtils, MainCamera mainCamera, PluginConfig pluginConfig, TimeTweeningManager timeTweeningManager, NsfwConfirmModalController nsfwConfirmModalController)
            : base(uiUtils, imageUtils, mainCamera, pluginConfig, timeTweeningManager, nsfwConfirmModalController)
        {
        }

        public void ShowModal(Transform parentTransform)
        {
            ShowModal(parentTransform, this);
            MoreSettingsTab.IsVisible = false;
        }
    }
}