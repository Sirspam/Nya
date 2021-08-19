using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using Nya.Configuration;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Zenject;
using HMUI;

namespace Nya.UI.ViewControllers
{
    class ViewController1 : IInitializable, IDisposable
    {
        public byte[] nyaImageBytes;
        public string nyaImageEndpoint;
        public string folderPath = Environment.CurrentDirectory + "/UserData/Nya";
        public class NekoLifeEntry
        { 
            [JsonProperty("url")]
            public string Url { get; set; }
        }
        public Task<byte[]> DownloadFileToBytesAsync(string url)
        {
            Uri uri = new Uri(url);
            using var webClient = new WebClient();
            return webClient.DownloadDataTaskAsync(uri);
        }
        public async Task<byte[]> GetImage(string endpoint)
        {
            var response = await DownloadFileToBytesAsync($"https://nekos.life/api/v2/img/{endpoint}");
            var endpointResult = JsonConvert.DeserializeObject<NekoLifeEntry>(Encoding.UTF8.GetString(response));
            nyaImageEndpoint = endpointResult.Url.Split('/').Last();
            return await DownloadFileToBytesAsync(endpointResult.Url);
        }
        public void Initialize()
        {
            GameplaySetup.instance.AddTab("Nya", "Nya.UI.Views.View1.bsml", this);
        }

        #region values
        [UIValue("nsfwCheck")]
        public bool nsfwCheck
        {
            get => PluginConfig.Instance.NSFW;
            set => PluginConfig.Instance.NSFW = value;
        }
        #endregion

        #region components
        [UIComponent("nyaImage")]
        public ImageView nyaImage;

        [UIComponent("nyaButton")]
        public UnityEngine.UI.Button nyaButton;

        [UIComponent("nyaDownloadButton")]
        public UnityEngine.UI.Button nyaDownloadButton;
        #endregion components

        #region actions
        [UIAction("nya-click")]
        public async void NyaClicked()
        {
            if (nsfwCheck) // NSFW
            {
                nyaButton.interactable = false;
                nyaImageBytes = await GetImage("lewd");
                nyaImage.sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(nyaImageBytes);
                nyaButton.interactable = true;
            }
            else // SFW
            {
                nyaButton.interactable = false;
                nyaImageBytes = await GetImage("neko");
                nyaImage.sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(nyaImageBytes);
                nyaButton.interactable = true;
            }

        }

        [UIAction("#post-parse")]
        public async void NyaPostParse()
        {
            NyaClicked();
        }

        [UIAction("nya-nsfw")]
        public void nsfwToggle(bool value)
        {
            nsfwCheck = value;
        }

        [UIAction("nya-download-click")]
        public void downloadNya()
        {
            File.WriteAllBytes($"{folderPath}/{nyaImageEndpoint}", nyaImageBytes);
        }
        #endregion actions

        public void Dispose()
        {
            if(GameplaySetup.instance!=null)
            {
                GameplaySetup.instance.RemoveTab("Nya");
            }
        }
    }
}
