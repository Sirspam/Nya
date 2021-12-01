using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.GameplaySetup;
using HMUI;
using IPA.Utilities;
using Nya.Configuration;
using Nya.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using UnityEngine.SceneManagement;
using BeatSaberMarkupLanguage.Components;

namespace Nya.UI.ViewControllers
{
    public abstract class SettingsModalController : IInitializable, INotifyPropertyChanged
    {
        protected readonly NsfwConfirmModalController nsfwConfirmModalController;
        protected readonly SettingsViewController settingsViewController;
        protected readonly UIUtils uiUtils;

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsModalController(NsfwConfirmModalController nsfwConfirmModalController, SettingsViewController settingsViewController, UIUtils uiUtils)
        {
            this.nsfwConfirmModalController = nsfwConfirmModalController;
            this.settingsViewController = settingsViewController;
            this.uiUtils = uiUtils;
        }

        #region components

        [UIComponent("root")]
        private readonly RectTransform RootTransform;

        [UIComponent("modal")]
        protected ModalView ModalView;

        [UIComponent("modal")]
        private readonly RectTransform ModalTransform;
        
        [UIComponent("more-settings-tab")]
        protected readonly Tab MoreSettingsTab;

        [UIComponent("nya-download-button")]
        protected readonly UnityEngine.UI.Button NyaDownloadButton;

        [UIComponent("nya-copy-button")]
        protected readonly UnityEngine.UI.Button NyaCopyButton;

        [UIComponent("nsfw-checkbox")]
        private readonly RectTransform NsfwCheckbox;

        [UIComponent("sfw-dropdown")]
        private readonly DropDownListSetting SfwDropDownListSetting;

        [UIComponent("nsfw-dropdown")]
        private readonly DropDownListSetting NsfwDropDownListSetting;

        [UIComponent("api-dropdown")]
        protected readonly Transform ApiDropDownTransform;

        [UIComponent("sfw-dropdown")]
        protected readonly Transform SfwDropDownTransform;

        [UIComponent("nsfw-dropdown")]
        protected readonly Transform NsfwDropDownTransform;

        [UIComponent("show-handle-checkbox")]
        private readonly GenericInteractableSetting ShowHandleCheckbox;

        #endregion components

        #region values

        [UIValue("nya-nsfw-check")]
        protected bool NsfwCheck
        {
            get => PluginConfig.Instance.Nsfw;
            set
            {
                PluginConfig.Instance.Nsfw = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NsfwCheck)));
            }
        }

        [UIValue("api-list")]
        protected List<object> apiList = new List<object>();

        [UIValue("api-value")]
        protected string APIValue
        {
            get => PluginConfig.Instance.SelectedAPI;
            set => PluginConfig.Instance.SelectedAPI = value;
        }

        [UIValue("sfw-list")]
        protected List<object> sfwList = new List<object>();

        [UIValue("sfw-value")]
        protected string SfwValue
        {
            get => PluginConfig.Instance.SelectedEndpoints[APIValue].SelectedSfwEndpoint;
            set => PluginConfig.Instance.SelectedEndpoints[APIValue].SelectedSfwEndpoint = value;
        }

        [UIValue("nsfw-list")]
        protected List<object> nsfwList = new List<object>();

        [UIValue("nsfw-value")]
        protected string NsfwValue
        {
            get => PluginConfig.Instance.SelectedEndpoints[APIValue].SelectedNsfwEndpoint;
            set => PluginConfig.Instance.SelectedEndpoints[APIValue].SelectedNsfwEndpoint = value;
        }

        [UIValue("in-menu")]
        protected bool InMenu
        {
            get => PluginConfig.Instance.InMenu;
            set => PluginConfig.Instance.InMenu = value;
        }

        [UIValue("in-pause")]
        protected bool InPause
        {
            get => PluginConfig.Instance.InPause;
            set => PluginConfig.Instance.InPause = value;
        }

        [UIValue("show-handle")]
        protected bool PauseHandle
        {
            get => PluginConfig.Instance.ShowHandle;
            set => PluginConfig.Instance.ShowHandle = value;
        }

        #endregion values

        [UIParams]
        protected readonly BSMLParserParams parserParams;

        public void Initialize()
        {
            SetupLists();
        }

        protected void Parse(Transform parentTransform)
        {
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.SettingsModal.bsml"), parentTransform.gameObject, this);
            ModalView.SetField("_animateParentCanvas", true);
            ApiDropDownTransform.Find("DropdownTableView").GetComponent<ModalView>().SetField("_animateParentCanvas", false);
            SfwDropDownTransform.Find("DropdownTableView").GetComponent<ModalView>().SetField("_animateParentCanvas", false);
            NsfwDropDownTransform.Find("DropdownTableView").GetComponent<ModalView>().SetField("_animateParentCanvas", false);
        }

        public void ShowModal(Transform parentTransform)
        {
            Parse(parentTransform);
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");

            var root = parentTransform.root;
            if (root.name == "NyaMenuFloatingScreen" || root.name == "NyaGameFloatingScreen")
            {
                foreach (HoverHint hoverComponent in root.gameObject.GetComponentsInChildren<HoverHint>())
                {
                    hoverComponent.enabled = false;
                }
            }

            if (ImageUtils.nyaImageURL.EndsWith(".gif") || ImageUtils.nyaImageURL.EndsWith(".apng"))
            {
                NyaCopyButton.interactable = false;
            }
            else
            {
                NyaCopyButton.interactable = true;
            }
        }

        public void HideModal()
        {
            if (ModalTransform != null)
            {
                ModalTransform.GetComponent<ModalView>().Hide(false);
                nsfwConfirmModalController.HideModal();
            }
        }

        #region actions

        [UIAction("nya-download-click")]
        protected void DownloadNya()
        {
            uiUtils.ButtonUnderlineClick(NyaDownloadButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
            ImageUtils.DownloadNyaImage();
        }

        [UIAction("nya-copy-click")]
        protected void CopyNya()
        {
            uiUtils.ButtonUnderlineClick(NyaCopyButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
            ImageUtils.CopyNyaImage();
        }

        [UIAction("nya-nsfw-changed")]
        protected void NsfwToggle(bool value)
        {
            if (value && !PluginConfig.Instance.SkipNsfw)
            {
                nsfwConfirmModalController.ShowModal(NsfwCheckbox, NsfwConfirmYes, NsfwConfirmNo);
            }
            else
            {
                NsfwCheck = value;
            }
        }

        [UIAction("api-change")]
        protected void ApiChange(string value)
        {
            APIValue = value;
            UpdateLists();
        }

        [UIAction("sfw-change")]
        protected void SfwChange(string value)
        {
            SfwValue = value;
            PluginConfig.Instance.SelectedEndpoints[APIValue].SelectedSfwEndpoint = value;
            PluginConfig.Instance.Changed();
        }

        [UIAction("nsfw-change")]
        protected void NsfwChange(string value)
        {
            NsfwValue = value;
            PluginConfig.Instance.Changed();
        }

        [UIAction("in-menu-changed")]
        protected void InMenuChanged(bool value)
        {
            GameplaySetup.instance.SetTabVisibility("Nya", value);
            RootTransform.root.gameObject.SetActive(!value);
            // if (rootTransform.root.name == "NyaMenuFloatingScreen")
            
            

        }

        [UIAction("in-pause-changed")]
        protected async void InPauseChanged(bool value)
        {
            if (RootTransform.root.name == "NyaGameFloatingScreen")
            {
                var path = "Nya.Resources.Chocola_Wave.png";
                if (new System.Random().Next(0, 11) == 0)
                {
                    path = "Nya.Resources.Peace.png";
                }
                Utilities.GetData(path, (byte[] data) => RootTransform.root.transform.Find("BSMLBackground").Find("BSMLVerticalLayoutGroup").Find("BSMLImage").GetComponent<ImageView>().sprite = Utilities.LoadSpriteRaw(data)); // .Find("Some Bitches")
                parserParams.EmitEvent("close-modal");
                RootTransform.root.GetChild(1).gameObject.SetActive(false);
                await Task.Delay(300);
                uiUtils.CanvasFadeOut(RootTransform.root.GetComponent<CanvasGroup>(), 2.5f);
                await Task.Delay(2500); // I don't know how to properly wait for the tweening to finish 😔
                RootTransform.root.gameObject.SetActive(false);
            }
        }

        [UIAction("show-handle-changed")]
        protected void ShowHandleChanged(bool value)
        {
            RootTransform.root.GetChild(1).gameObject.SetActive(value); // Gets the handle child
        }

        #endregion actions

        protected void NsfwConfirmYes()
        {
            NsfwCheck = true;
            PluginConfig.Instance.Changed();
        }

        protected void NsfwConfirmNo()
        {
            if (NsfwCheck) // Stops editing the config if the nsfw value is already false
            {
                NsfwCheck = false;
                PluginConfig.Instance.Changed();
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NsfwCheck)));
        }

        protected void SetupLists()
        {
            foreach (var api in WebAPIs.APIs.Keys)
            {
                apiList.Add(api);
            }
            foreach (var endpoint in WebAPIs.APIs[APIValue].SfwEndpoints)
            {
                sfwList.Add(endpoint);
            }
            foreach (var endpoint in WebAPIs.APIs[APIValue].NsfwEndpoints)
            {
                nsfwList.Add(endpoint);
            }
        }

        protected void UpdateLists()
        {
            SfwDropDownListSetting.values.Clear();
            NsfwDropDownListSetting.values.Clear();
            foreach (var endpoint in WebAPIs.APIs[APIValue].SfwEndpoints)
            {
                SfwDropDownListSetting.values.Add(endpoint);
            }
            foreach (var endpoint in WebAPIs.APIs[APIValue].NsfwEndpoints)
            {
                NsfwDropDownListSetting.values.Add(endpoint);
            }
            SfwDropDownListSetting.UpdateChoices();
            NsfwDropDownListSetting.UpdateChoices();
        }
    }
}