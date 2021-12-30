using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nya.UI.ViewControllers
{
    internal abstract class NyaViewController
    {
        protected readonly PluginConfig Config;
        protected readonly ImageUtils ImageUtils;

        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        protected bool autoNyaToggle = false;
        protected bool autoNyaCooldown = false;

        protected NyaViewController(PluginConfig config, ImageUtils imageUtils)
        {
            Config = config;
            ImageUtils = imageUtils;
        }

        #region components

        [UIComponent("root")]
        internal readonly RectTransform rootTransform;

        [UIComponent("nya-image")]
        internal readonly ImageView nyaImage;

        [UIComponent("nya-button")]
        internal readonly Button nyaButton;

        [UIComponent("auto-button")]
        internal readonly Button nyaAutoButton;

        [UIComponent("auto-button")]
        internal readonly TextMeshProUGUI nyaAutoText;

        [UIComponent("settings-button")]
        internal readonly Button nyaSettingsButton;

        [UIComponent("settings-button")]
        internal readonly RectTransform settingsButtonTransform;

        #endregion components

        #region actions

        [UIAction("#post-parse")]
        protected void NyaPostParse()
        {
            nyaButton.interactable = false;
            ImageUtils.LoadNyaImage(nyaImage);
            nyaButton.interactable = true;
        }

        [UIAction("nya-click")]
        protected void NyaClicked()
        {
            nyaButton.interactable = false;
            ImageUtils.GetNewNyaImage(nyaImage);
            nyaButton.interactable = true;
        }

        [UIAction("nya-auto-clicked")]
        protected async void AutoNya()
        {
            if (autoNyaCooldown)
            {
                return;
            }
            autoNyaCooldown = true;

            autoNyaToggle = !autoNyaToggle;
            if (autoNyaToggle) // On
            {
                AutoNyaCooldownHandler(); // This isn't suppoed to be awaited I swear, please Mr green swiggly line go away you're scaring me
                nyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = Color.green;
                nyaButton.interactable = false;
                while (autoNyaToggle)
                {
                    await semaphore.WaitAsync();
                    ImageUtils.GetNewNyaImage(nyaImage);
                    await Task.Delay(Config.AutoNyaWait * 1000);
                    semaphore.Release();
                }
                // This is a neat little thing I wanted to do but couldn't get it to work ):
                // Might come back to it in the future
                //await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew( async () =>
                //{
                //    var third = (_config.AutoNyaWait * 1000) / 3;
                //    while (autoNyaToggle)
                //    {
                //        await semaphore.WaitAsync();
                //        await ImageUtils.LoadNewNyaSprite(nyaImage);
                //        nyaAutoText.text = ".";
                //        await Task.Delay(third);
                //        nyaAutoText.text = ". .";
                //        await Task.Delay(third);
                //        nyaAutoText.text = ". . .";
                //        await Task.Delay(third);
                //        nyaAutoText.text = "Nya!";
                //        semaphore.Release();
                //    }
                //}, cancellationTokenSource.Token);
            }
            else // Off
            {
                // cancellationTokenSource.Cancel();
                nyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f); // Beatgames why 0.502
                nyaAutoText.text = "Auto Nya";
                nyaButton.interactable = true;
                autoNyaCooldown = false;
            }
        }

        private async void AutoNyaCooldownHandler()
        {
            await Task.Delay(1000);
            autoNyaCooldown = false;
        }

        #endregion actions
    }
}