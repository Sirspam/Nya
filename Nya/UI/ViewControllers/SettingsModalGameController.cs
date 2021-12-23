using Nya.Utils;
using UnityEngine;

namespace Nya.UI.ViewControllers
{
    public class SettingsModalGameController : SettingsModalController
    {
        public SettingsModalGameController(MainCamera mainCamera, NsfwConfirmModalController nsfwConfirmModalController, UIUtils uiUtils) : base(mainCamera, nsfwConfirmModalController, uiUtils)
        {
        }

        public void ShowModal(Transform parentTransform)
        {
            ShowModal(parentTransform, this);
            MoreSettingsTab.IsVisible = false;
        }
    }
}