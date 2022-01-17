using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using Nya.Configuration;
using Nya.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Nya.UI.ViewControllers
{
    public abstract class SettingsModalController : IInitializable, INotifyPropertyChanged
    {
        protected MainCamera mainCamera;
        protected readonly NsfwConfirmModalController nsfwConfirmModalController;
        protected readonly UIUtils uiUtils;

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsModalController(MainCamera mainCamera, NsfwConfirmModalController nsfwConfirmModalController, UIUtils uiUtils)
        {
            this.mainCamera = mainCamera;
            this.nsfwConfirmModalController = nsfwConfirmModalController;
            this.uiUtils = uiUtils;
        }

        #region components

        [UIComponent("root")]
        protected readonly RectTransform RootTransform;

        [UIComponent("modal")]
        protected readonly ModalView ModalView;

        [UIComponent("modal")]
        protected readonly RectTransform ModalTransform;
        
        [UIComponent("nya-tab")]
        protected readonly Tab NyaTab;
        
        [UIComponent("api-tab")]
        protected readonly Tab ApiTab;
        
        [UIComponent("screen-tab")]
        protected readonly Tab ScreenTab;

        [UIComponent("more-settings-tab")]
        protected readonly Tab MoreSettingsTab;

        [UIComponent("nya-download-button")]
        protected readonly Button NyaDownloadButton;

        [UIComponent("nya-copy-button")]
        protected readonly Button NyaCopyButton;

        [UIComponent("nsfw-checkbox")]
        protected readonly RectTransform NsfwCheckbox;

        [UIComponent("sfw-dropdown")]
        protected readonly DropDownListSetting SfwDropDownListSetting;

        [UIComponent("nsfw-dropdown")]
        protected readonly DropDownListSetting NsfwDropDownListSetting;

        [UIComponent("api-dropdown")]
        protected readonly Transform ApiDropDownTransform;

        [UIComponent("sfw-dropdown")]
        protected readonly Transform SfwDropDownTransform;

        [UIComponent("nsfw-dropdown")]
        protected readonly Transform NsfwDropDownTransform;

        [UIComponent("face-headset-button")]
        protected readonly Button FaceHeadsetButton;

        [UIComponent("face-forwards-button")]
        protected readonly Button FaceForwardsButton;

        [UIComponent("reset-position-button")]
        protected readonly Button ResetPositionButton;

        [UIComponent("show-handle-checkbox")]
        protected readonly GenericInteractableSetting ShowHandleCheckbox;

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

        protected void BaseParse(Transform parentTransform, object host)
        {
            if (!ModalView)
            {
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.SettingsModal.bsml"), parentTransform.gameObject, host);
                ModalView.SetField("_animateParentCanvas", true);
                ApiDropDownTransform.Find("DropdownTableView").GetComponent<ModalView>().SetField("_animateParentCanvas", false);
                SfwDropDownTransform.Find("DropdownTableView").GetComponent<ModalView>().SetField("_animateParentCanvas", false);
                NsfwDropDownTransform.Find("DropdownTableView").GetComponent<ModalView>().SetField("_animateParentCanvas", false);
            }
        }

        public void ShowModal(Transform parentTransform, object host)
        {
            BaseParse(parentTransform, host);
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");
            
            NyaTab.gameObject.SetActive(true);
            ApiTab.gameObject.SetActive(false);
            ScreenTab.gameObject.SetActive(false);
            MoreSettingsTab.gameObject.SetActive(false);

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

        [UIAction("nya-download-clicked")]
        protected void DownloadNya()
        {
            uiUtils.ButtonUnderlineClick(NyaDownloadButton.gameObject);
            Task.Run(() => ImageUtils.DownloadNyaImage());
        }

        [UIAction("nya-copy-clicked")]
        protected void CopyNya()
        {
            uiUtils.ButtonUnderlineClick(NyaCopyButton.gameObject);
            Task.Run(() => ImageUtils.CopyNyaImage());
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

        [UIAction("face-headset-clicked")]
        protected void FaceHeadsetClicked()
        {
            uiUtils.ButtonUnderlineClick(FaceHeadsetButton.gameObject);
            RootTransform.root.gameObject.transform.LookAt(mainCamera.camera.transform);
            RootTransform.root.gameObject.transform.Rotate(0f, 180f, 0, Space.Self); // Nya decides that it's shy and faces away from the user, so we do a little flipping
        }

        [UIAction("face-forwards-clicked")]
        protected void ReseteRotationClicked()
        {
            uiUtils.ButtonUnderlineClick(FaceForwardsButton.gameObject);
            Vector3 rotation = RootTransform.root.gameObject.transform.rotation.eulerAngles;
            RootTransform.root.gameObject.transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        }

        [UIAction("reset-position-clicked")]
        protected void ResetPositionClicked()
        {
            uiUtils.ButtonUnderlineClick(ResetPositionButton.gameObject);
            RootTransform.root.gameObject.transform.position = new Vector3(0f, 3.65f, 4f);
            RootTransform.root.gameObject.transform.rotation = Quaternion.Euler(new Vector3(335f, 0f, 0f));
            if (RootTransform.root.name == "NyaGameFloatingScreen" && PluginConfig.Instance.SeperatePositions)
            {
                PluginConfig.Instance.PausePosition = RootTransform.root.gameObject.transform.position;
                PluginConfig.Instance.PauseRotation = RootTransform.root.gameObject.transform.eulerAngles;
            }
            else
            {
                PluginConfig.Instance.MenuPosition = RootTransform.root.gameObject.transform.position;
                PluginConfig.Instance.MenuRotation = RootTransform.root.gameObject.transform.eulerAngles;
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
            if (NsfwCheck) // Stops from editing the config if the nsfw value is already false
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