using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Nya.UI.ViewControllers
{
    internal class NyaViewController
    {
        protected readonly SettingsModalController settingsModalController;
        protected static SemaphoreSlim semaphore;

        protected bool autoNyaToggle = false;
        protected bool AutoNyaCooldown = false;

        public NyaViewController(SettingsModalController settingsModalController)
        {
            this.settingsModalController = settingsModalController;
            semaphore = new SemaphoreSlim(1);
        }

        #region components
        [UIComponent("root")]
        internal readonly RectTransform rootTransform;

        [UIComponent("nyaImage")]
        internal readonly ImageView nyaImage;

        [UIComponent("nyaButton")]
        internal readonly UnityEngine.UI.Button nyaButton;

        [UIComponent("nyaDownloadButton")]
        internal readonly UnityEngine.UI.Button nyaDownloadButton;

        [UIComponent("nyaAutoButton")]
        internal readonly UnityEngine.UI.Button nyaAutoButton;

        [UIComponent("settingsButton")]
        internal readonly RectTransform settingsButtonTransform;
        #endregion

        #region actions
        [UIAction("#post-parse")]
        protected async void NyaPostParse()
        {
            await ImageUtils.LoadNyaSprite(nyaImage);
        }

        [UIAction("nya-click")]
        protected async void NyaClicked()
        {
            nyaButton.interactable = false;
            await ImageUtils.LoadNewNyaSprite(nyaImage);
            nyaButton.interactable = true;
        }

        [UIAction("nya-auto-clicked")]
        protected async void autoNya()
        {
            if (AutoNyaCooldown)
            {
                return;
            }
            AutoNyaCooldown = true;

            autoNyaToggle = !autoNyaToggle;
            if (autoNyaToggle) // On
            {
                autoNyaCooldownHandler(); // Stops users from spamming Auto Nya and by extension spamming whatever API is selected
                nyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = Color.green;
                nyaButton.interactable = false;
                while (autoNyaToggle)
                {
                    await semaphore.WaitAsync();
                    await ImageUtils.LoadNewNyaSprite(nyaImage);
                    await Task.Delay(PluginConfig.Instance.autoNyaWait * 1000);
                    semaphore.Release();
                }
            }
            else // Off
            {
                nyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f); // Beatgames why 0.502
                nyaButton.interactable = true;
                AutoNyaCooldown = false;
            }
        }

        private async Task autoNyaCooldownHandler()
        {
            await Task.Delay(1000);
            AutoNyaCooldown = false;
        }

        [UIAction("settings-button-clicked")]
        protected async void SettingsButtonClicked()
        {
            if (autoNyaToggle)
            {
                autoNya();
            }
            settingsModalController.ShowModal(settingsButtonTransform);
        }
        #endregion
    }
}
