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
    internal abstract class SettingsModalController : IInitializable, INotifyPropertyChanged
    {
        private readonly ImageUtils _imageUtils;
        private readonly UIUtils _uiUtils;
        private readonly MainCamera _mainCamera;
        private readonly NsfwConfirmModalController _nsfwConfirmModalController;

        protected readonly PluginConfig Config;

        public event PropertyChangedEventHandler PropertyChanged;

        protected SettingsModalController(PluginConfig config, ImageUtils imageUtils, UIUtils uiUtils, NsfwConfirmModalController nsfwConfirmModalController, MainCamera mainCamera)
        {
            _imageUtils = imageUtils;
            _mainCamera = mainCamera;
            _nsfwConfirmModalController = nsfwConfirmModalController;
            _uiUtils = uiUtils;

            Config = config;
        }

        #region components

        [UIComponent("root")]
        protected readonly RectTransform RootTransform;

        [UIComponent("modal")]
        protected readonly ModalView ModalView;

        [UIComponent("modal")]
        protected readonly RectTransform ModalTransform;

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

        [UIComponent("reset-rotation-button")]
        protected readonly Button ResetRotationButton;

        [UIComponent("reset-position-button")]
        protected readonly Button ResetPositionButton;

        [UIComponent("show-handle-checkbox")]
        protected readonly GenericInteractableSetting ShowHandleCheckbox;

        #endregion components

        #region values

        [UIValue("nya-nsfw-check")]
        protected bool NsfwCheck
        {
            get => Config.Nsfw;
            set
            {
                Config.Nsfw = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NsfwCheck)));
            }
        }

        [UIValue("api-list")]
        protected List<object> apiList = new List<object>();

        [UIValue("api-value")]
        protected string APIValue
        {
            get => Config.SelectedAPI;
            set => Config.SelectedAPI = value;
        }

        [UIValue("sfw-list")]
        protected List<object> sfwList = new List<object>();

        [UIValue("sfw-value")]
        protected string SfwValue
        {
            get => Config.SelectedEndpoints[APIValue].SelectedSfwEndpoint;
            set => Config.SelectedEndpoints[APIValue].SelectedSfwEndpoint = value;
        }

        [UIValue("nsfw-list")]
        protected List<object> nsfwList = new List<object>();

        [UIValue("nsfw-value")]
        protected string NsfwValue
        {
            get => Config.SelectedEndpoints[APIValue].SelectedNsfwEndpoint;
            set => Config.SelectedEndpoints[APIValue].SelectedNsfwEndpoint = value;
        }

        [UIValue("show-handle")]
        protected bool PauseHandle
        {
            get => Config.ShowHandle;
            set => Config.ShowHandle = value;
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

            if (_imageUtils.NyaImageURL.EndsWith(".gif") || _imageUtils.NyaImageURL.EndsWith(".apng"))
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
                _nsfwConfirmModalController.HideModal();
            }
        }

        #region actions

        [UIAction("nya-download-clicked")]
        protected void DownloadNya()
        {
            _uiUtils.ButtonUnderlineClick(NyaDownloadButton.gameObject);
            Task.Run(() => _imageUtils.DownloadNyaImage());
        }

        [UIAction("nya-copy-clicked")]
        protected void CopyNya()
        {
            _uiUtils.ButtonUnderlineClick(NyaCopyButton.gameObject);
            Task.Run(() => _imageUtils.CopyNyaImage());
        }

        [UIAction("nya-nsfw-changed")]
        protected void NsfwToggle(bool value)
        {
            if (value && !Config.SkipNsfw)
            {
                _nsfwConfirmModalController.ShowModal(NsfwCheckbox, NsfwConfirmYes, NsfwConfirmNo);
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
            Config.SelectedEndpoints[APIValue].SelectedSfwEndpoint = value;
            Config.Changed();
        }

        [UIAction("nsfw-change")]
        protected void NsfwChange(string value)
        {
            NsfwValue = value;
            Config.Changed();
        }

        [UIAction("face-headset-clicked")]
        protected void FaceHeadsetClicked()
        {
            _uiUtils.ButtonUnderlineClick(FaceHeadsetButton.gameObject);
            var transform = RootTransform.root.gameObject.transform;
            transform.LookAt(_mainCamera.camera.transform);
            transform.Rotate(0f, 180f, 0, Space.Self); // Nya decides that it's shy and faces away from the user, so we do a little flipping
        }

        [UIAction("reset-rotation-clicked")]
        protected void ResetRotationClicked()
        {
            _uiUtils.ButtonUnderlineClick(ResetRotationButton.gameObject);
            var transform = RootTransform.root.gameObject.transform;
            Vector3 rotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        }

        [UIAction("reset-position-clicked")]
        protected void ResetPositionClicked()
        {
            _uiUtils.ButtonUnderlineClick(ResetPositionButton.gameObject);
            var gameObjectTransform = RootTransform.root.gameObject.transform;
            gameObjectTransform.position = new Vector3(0f, 3.65f, 4f);
            gameObjectTransform.rotation = Quaternion.Euler(new Vector3(335f, 0f, 0f));
            if (RootTransform.root.name == "NyaGameFloatingScreen" && Config.SeperatePositions)
            {
                Config.PausePosition = gameObjectTransform.position;
                Config.PauseRotation = gameObjectTransform.eulerAngles;
            }
            else
            {
                Config.MenuPosition = gameObjectTransform.position;
                Config.MenuRotation = gameObjectTransform.eulerAngles;
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
            Config.Changed();
        }

        protected void NsfwConfirmNo()
        {
            if (NsfwCheck) // Stops from editing the config if the nsfw value is already false
            {
                NsfwCheck = false;
                Config.Changed();
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