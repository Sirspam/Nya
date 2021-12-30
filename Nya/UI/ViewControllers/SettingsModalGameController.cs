using Nya.Configuration;
using Nya.Utils;
using UnityEngine;

namespace Nya.UI.ViewControllers
{
    internal class SettingsModalGameController : SettingsModalController
    {
        public SettingsModalGameController(PluginConfig config, ImageUtils imageUtils, UIUtils uiUtils, NsfwConfirmModalController nsfwConfirmModalController, MainCamera mainCamera)
            : base(config, imageUtils, uiUtils, nsfwConfirmModalController, mainCamera)
        {
        }

        public void ShowModal(Transform parentTransform)
        {
            ShowModal(parentTransform, this);
            MoreSettingsTab.IsVisible = false;
        }
    }
}