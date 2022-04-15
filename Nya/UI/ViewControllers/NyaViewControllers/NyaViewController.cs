using System.Threading;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nya.UI.ViewControllers.NyaViewControllers
{
    internal abstract class NyaViewController
    {
        protected readonly PluginConfig PluginConfig;
        protected readonly ImageUtils ImageUtils;

        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);

        protected bool AutoNyaToggle;
        protected bool AutoNyaCooldown;

        protected NyaViewController(PluginConfig pluginConfig, ImageUtils imageUtils)
        {
            PluginConfig = pluginConfig;
            ImageUtils = imageUtils;
        }

        #region components

        [UIComponent("root")]
        internal readonly RectTransform RootTransform = null!;

        [UIComponent("nya-image")]
        internal readonly ImageView NyaImage = null!;

        [UIComponent("nya-button")]
        internal readonly Button NyaButton = null!;

        [UIComponent("auto-button")]
        internal readonly Button NyaAutoButton = null!;

        [UIComponent("auto-button")]
        internal readonly TextMeshProUGUI NyaAutoText = null!;

        [UIComponent("settings-button")]
        internal readonly Button NyaSettingsButton = null!;

        [UIComponent("settings-button")]
        internal readonly RectTransform SettingsButtonTransform = null!;

        #endregion components

        #region actions

        [UIAction("#post-parse")]
        protected void NyaPostParse()
        {
            NyaButton.interactable = false;
            ImageUtils.LoadCurrentNyaImage(NyaImage, () => NyaButton.interactable = true);
        }

        [UIAction("nya-click")]
        protected void NyaClicked()
        {
            NyaButton.interactable = false;
            ImageUtils.LoadNewNyaImage(NyaImage, () => NyaButton.interactable = true);
        }

        [UIAction("nya-auto-clicked")]
        protected async void AutoNya()
        {
            if (AutoNyaCooldown)
            {
                return;
            }

            AutoNyaCooldown = true;

            AutoNyaToggle = !AutoNyaToggle;
            if (AutoNyaToggle)
            {
                AutoNyaCooldownHandler(); // This isn't suppoed to be awaited I swear, please Mr green swiggly line go away you're scaring me
                NyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = Color.green;
                NyaButton.interactable = false;
                while (AutoNyaToggle)
                {
                    await Semaphore.WaitAsync();
                    ImageUtils.LoadNewNyaImage(NyaImage, null);
                    await Task.Delay(PluginConfig.AutoNyaWait * 1000);
                    Semaphore.Release();
                }
            }
            else
            {
                NyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f); // Beatgames why 0.502
                NyaAutoText.text = "Auto Nya";
                NyaButton.interactable = true;
                AutoNyaCooldown = false;
            }
        }

        private async void AutoNyaCooldownHandler()
        {
            await Task.Delay(1000);
            AutoNyaCooldown = false;
        }

        #endregion actions
    }
}