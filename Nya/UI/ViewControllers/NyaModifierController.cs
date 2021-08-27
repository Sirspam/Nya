using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Animations;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using Nya.Configuration;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Zenject;
using HMUI;
using UnityEngine;
using TMPro;

namespace Nya.UI.ViewControllers
{
    public class NyaModifierController : IInitializable, IDisposable
    {
        private readonly HttpClient client = new HttpClient();
        private SettingsModalController settingsModalController;
        private GameplaySetupViewController gameplaySetupViewController;
        private static byte[] nyaImageBytes;
        private static string nyaImageEndpoint;
        private static string nyaImageURL;
        private static string folderPath = Environment.CurrentDirectory + "/UserData/Nya";
        private bool autoNyaToggle = false;

        public static void downloadNya()
        {
            File.WriteAllBytes($"{folderPath}/{nyaImageEndpoint}", nyaImageBytes);
        }
        public static void copyNya()
        {
            using (MemoryStream ms = new MemoryStream(nyaImageBytes))
            {
                Bitmap bm = new Bitmap(ms);
                Clipboard.SetImage(bm); // Converts gifs to pngs because ???
            }
        }
        private class NekoLifeEntry
        { 
            [JsonProperty("url")]
            public string Url { get; set; }
        }
        private async Task<byte[]> GetWebDataToBytesAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException error)
            {
                Plugin.Log.Error($"Error getting data from {url}, Message: {error}");
                Utilities.GetData("Nya.Assets.Chocola_Dead.png", (byte[] data) =>
                {
                    nyaImageBytes = data;
                    nyaImage.sprite = Utilities.LoadSpriteRaw(data);
                });
                return null; // Breaks GetImageURL Task, prob an easy way to fix but I don't know it so I left it be for now.
            }
        }
        private async Task<string> GetImageURL(string endpoint)
        {
            var response = await GetWebDataToBytesAsync(PluginConfig.Instance.APIs[PluginConfig.Instance.selectedAPI].URL + endpoint);
            var endpointResult = JsonConvert.DeserializeObject<NekoLifeEntry>(Encoding.UTF8.GetString(response));
            nyaImageEndpoint = endpointResult.Url.Split('/').Last();
            return endpointResult.Url;
        }

        public async Task loadNyaSprite()
        {
            if (PluginConfig.Instance.NSFW) // NSFW
            {
                nyaImageURL = await GetImageURL(PluginConfig.Instance.APIs[PluginConfig.Instance.selectedAPI].selectedNSFW_Endpoint);
            }
            else // SFW
            {
                nyaImageURL = await GetImageURL(PluginConfig.Instance.APIs[PluginConfig.Instance.selectedAPI].selectedSFW_Endpoint);
            }
            Plugin.Log.Debug($"Loading from {nyaImageURL}");

            // Below is essentially BSML's SetImage method but adapted "better" for Nya
            // I didn't like that it would show a yucky loading gif >:(
            AnimationStateUpdater oldStateUpdater = nyaImage.GetComponent<AnimationStateUpdater>();
            if (oldStateUpdater != null)
                UnityEngine.Object.DestroyImmediate(oldStateUpdater);

            if (nyaImageURL.EndsWith(".gif") || nyaImageURL.EndsWith(".apng"))
            {
                AnimationStateUpdater stateUpdater = nyaImage.gameObject.AddComponent<AnimationStateUpdater>();
                stateUpdater.image = nyaImage;
                var data = await GetWebDataToBytesAsync(nyaImageURL);
                AnimationLoader.Process((nyaImageURL.EndsWith(".gif") || (nyaImageURL.EndsWith(".gif"))) ? AnimationType.GIF : AnimationType.APNG, data, (Texture2D tex, Rect[] uvs, float[] delays, int width, int height) =>
                {
                    nyaImageBytes = data; 
                    AnimationControllerData controllerData = AnimationController.instance.Register(nyaImageURL, tex, uvs, delays);
                    stateUpdater.controllerData = controllerData;
                });
            }
            else
            {
                AnimationStateUpdater stateUpdater = nyaImage.gameObject.AddComponent<AnimationStateUpdater>();
                stateUpdater.image = nyaImage;
                var data = await GetWebDataToBytesAsync(nyaImageURL);
                if (stateUpdater != null)
                    UnityEngine.Object.DestroyImmediate(stateUpdater);
                nyaImageBytes = data;
                nyaImage.sprite = Utilities.LoadSpriteRaw(data);
            }
        }

        #region components
        [UIComponent("nyaImage")]
        private readonly ImageView nyaImage;

        [UIComponent("nyaButton")]
        private readonly UnityEngine.UI.Button nyaButton;

        [UIComponent("nyaDownloadButton")]
        private readonly UnityEngine.UI.Button nyaDownloadButton;

        [UIComponent("nyaAutoButton")]
        private readonly TextMeshProUGUI nyaAutoButton;

        [UIComponent("settingsButton")]
        private readonly RectTransform settingsButtonTransform;
        #endregion

        #region actions
        [UIAction("nya-click")]
        public async void NyaClicked()
        {
            nyaButton.interactable = false;
            await loadNyaSprite();
            nyaButton.interactable = true;
        }

        [UIAction("#post-parse")]
        public async void NyaPostParse()
        {
            await loadNyaSprite();
        }

        [UIAction("nya-auto-clicked")]
        public async void autoNya()
        {
            autoNyaToggle = !autoNyaToggle;
            if (autoNyaToggle) // On
            {
                nyaAutoButton.faceColor = new Color32(46, 204, 113, 255); // Green
                nyaButton.interactable = false;
                while (autoNyaToggle)
                {
                    await loadNyaSprite();
                    await Task.Delay(4000);
                }
            }
            else // Off
            {
                nyaAutoButton.faceColor = new Color32(255, 255, 255, 255); // White
                nyaButton.interactable = true;
            }
        }

        [UIAction("settings-button-clicked")]
        public void SettingsButtonClicked()
        {
            settingsModalController.ShowModal(settingsButtonTransform);
        }
        #endregion

        public NyaModifierController(SettingsModalController settingsModalController, GameplaySetupViewController gameplaySetupViewController)
        {
            this.settingsModalController = settingsModalController;
            this.gameplaySetupViewController = gameplaySetupViewController;
        }

        private void GameplaySetupViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy)
        {
            if (autoNyaToggle)
            {
                autoNyaToggle = false;
                nyaAutoButton.faceColor = new Color32(255, 255, 255, 255); // White
                nyaButton.interactable = true;
            }
        }
        public void Initialize()
        {
            GameplaySetup.instance.AddTab("Nya", "Nya.UI.Views.NyaModifierView.bsml", this);
            gameplaySetupViewController.didDeactivateEvent += GameplaySetupViewController_didActivateEvent;
        }
        public void Dispose()
        {
            if(GameplaySetup.instance!=null)
            {
                GameplaySetup.instance.RemoveTab("Nya");
            }
            gameplaySetupViewController.didDeactivateEvent -= GameplaySetupViewController_didActivateEvent;
        }
    }
}
