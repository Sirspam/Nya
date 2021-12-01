using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Nya.UI.ViewControllers
{
    public abstract class NyaViewController
    {
        protected static SemaphoreSlim semaphore;
        // protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        protected bool autoNyaToggle = false;
        protected bool autoNyaCooldown = false;

        public NyaViewController()
        {
            semaphore = new SemaphoreSlim(1);
        }

        #region components

        [UIComponent("root")]
        internal readonly RectTransform rootTransform;

        [UIComponent("nya-image")]
        internal readonly ImageView nyaImage;

        [UIComponent("nya-button")]
        internal readonly UnityEngine.UI.Button nyaButton;

        [UIComponent("auto-button")]
        internal readonly UnityEngine.UI.Button nyaAutoButton;
        
        [UIComponent("auto-button")]
        internal readonly TextMeshProUGUI nyaAutoText;

        [UIComponent("settings-button")]
        internal readonly UnityEngine.UI.Button nyaSettingsButton;

        [UIComponent("settings-button")]
        internal readonly RectTransform settingsButtonTransform;

        #endregion components

        #region actions

        [UIAction("#post-parse")]
        protected async void NyaPostParse()
        {
            nyaButton.interactable = false;
            await ImageUtils.LoadNyaSprite(nyaImage);
            nyaButton.interactable = true;
        }

        [UIAction("nya-click")]
        protected async void NyaClicked()
        {
            nyaButton.interactable = false;
            await ImageUtils.LoadNewNyaSprite(nyaImage);
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
                    await ImageUtils.LoadNewNyaSprite(nyaImage);
                    await Task.Delay(PluginConfig.Instance.AutoNyaWait * 1000);
                    semaphore.Release();
                }
                // This is a neat little thing I wanted to do but couldn't get it to work ):
                // Might come back to it later
                //await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew( async () =>
                //{
                //    var third = (PluginConfig.Instance.AutoNyaWait * 1000) / 3;
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
        
        private async Task AutoNyaCooldownHandler()
        {
            await Task.Delay(1000);
            autoNyaCooldown = false;
        }

        #endregion actions
    }
}