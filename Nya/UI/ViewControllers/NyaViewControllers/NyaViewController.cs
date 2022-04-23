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
        protected bool AutoNyaToggle;
        private bool _autoNyaCooldown;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        protected readonly ImageUtils ImageUtils;
        protected readonly PluginConfig PluginConfig;

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
            if (_autoNyaCooldown)
            {
                return;
            }

            _autoNyaCooldown = true;
            AutoNyaToggle = !AutoNyaToggle;
            ImageUtils.AutoNyaActive = AutoNyaToggle;
            
            if (AutoNyaToggle)
            {
                AutoNyaCooldownHandler();
                NyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = Color.green;
                NyaButton.interactable = false;
                
                while (AutoNyaToggle)
                {
                    await _semaphore.WaitAsync();
                    await Task.Run(() => ImageUtils.LoadNewNyaImage(NyaImage, null));
                    await Task.Delay(PluginConfig.AutoNyaWait * 1000);
                    _semaphore.Release();
                }
            }
            else
            {
                NyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f); // Beatgames why 0.502
                NyaButton.interactable = true;
                _autoNyaCooldown = false;
            }
        }

        private async void AutoNyaCooldownHandler()
        {
            await Task.Delay(1000);
            _autoNyaCooldown = false;
        }

        #endregion actions

        public virtual void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}