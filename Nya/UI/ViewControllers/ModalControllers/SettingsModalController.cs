using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using Nya.Configuration;
using Nya.Utils;
using Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Component = UnityEngine.Component;

namespace Nya.UI.ViewControllers.ModalControllers
{
    internal abstract class SettingsModalController : IInitializable, INotifyPropertyChanged
    {
        private GameObject? _handle;
        private bool _parsed;

        private readonly UIUtils _uiUtils;
        private readonly ImageUtils _imageUtils;
        private readonly MainCamera _mainCamera;
        protected readonly PluginConfig PluginConfig;
        private readonly FloatingScreenUtils _floatingScreenUtils;
        private readonly TimeTweeningManager _timeTweeningManager;
        private readonly NsfwConfirmModalController _nsfwConfirmModalController;

        public event PropertyChangedEventHandler PropertyChanged = null!;

        protected SettingsModalController(UIUtils uiUtils, ImageUtils imageUtils, MainCamera mainCamera, PluginConfig pluginConfig, FloatingScreenUtils floatingScreenUtils, TimeTweeningManager timeTweeningManager, NsfwConfirmModalController nsfwConfirmModalController)
        {
            _uiUtils = uiUtils;
            _imageUtils = imageUtils;
            _mainCamera = mainCamera;
            PluginConfig = pluginConfig;
            _floatingScreenUtils = floatingScreenUtils;
            _timeTweeningManager = timeTweeningManager;
            _nsfwConfirmModalController = nsfwConfirmModalController;
        }

        #region components

        [UIComponent("root")]
        protected readonly RectTransform RootTransform = null!;

        [UIComponent("modal")] 
        public readonly ModalView ModalView = null!;

        [UIComponent("modal")]
        protected readonly RectTransform ModalTransform = null!;

        [UIComponent("settings-modal-tab-selector")]
        protected readonly TabSelector TabSelector = null!;
        
        [UIComponent("nya-tab")]
        protected readonly Tab NyaTab = null!;

        [UIComponent("api-tab")]
        protected readonly Tab ApiTab = null!;
        
        [UIComponent("screen-tab")]
        protected readonly Tab ScreenTab = null!;

        [UIComponent("more-settings-tab")]
        protected readonly Tab MoreSettingsTab = null!;

        [UIComponent("nya-download-button")]
        protected readonly Button NyaDownloadButton = null!;

        [UIComponent("nya-open-button")]
        protected readonly Button NyaOpenButton = null!;

        [UIComponent("nsfw-checkbox")]
        protected readonly GenericInteractableSetting NsfwCheckbox = null!;

        [UIComponent("api-dropdown")]
        protected readonly DropDownListSetting ApiDropDownListSetting = null!;
        
        [UIComponent("sfw-dropdown")]
        protected readonly DropDownListSetting SfwDropDownListSetting = null!;

        [UIComponent("nsfw-dropdown")]
        protected readonly DropDownListSetting NsfwDropDownListSetting = null!;

        [UIComponent("api-dropdown")]
        protected readonly Transform ApiDropDownTransform = null!;

        [UIComponent("sfw-dropdown")]
        protected readonly Transform SfwDropDownTransform = null!;

        [UIComponent("nsfw-dropdown")]
        protected readonly Transform NsfwDropDownTransform = null!;

        [UIComponent("face-headset-button")]
        protected readonly Button FaceHeadsetButton = null!;

        [UIComponent("set-upright-button")]
        protected readonly Button SetUprightButton = null!;

        [UIComponent("default-position-button")]
        protected readonly Button DefaultPositionButton = null!;

        [UIComponent("show-handle-checkbox")]
        protected readonly GenericInteractableSetting ShowHandleCheckbox = null!;

        #endregion components

        #region values

        [UIValue("nsfw-features")]
        protected bool NsfwFeatures => PluginConfig.NsfwFeatures;

        [UIValue("show-handle")]
        protected bool PauseHandle
        {
            get => PluginConfig.ShowHandle;
            set => PluginConfig.ShowHandle = value;
        }

        #endregion values

        [UIParams]
        protected readonly BSMLParserParams ParserParams = null!;

        private void BaseParse(Component parentTransform, object host)
        {
            if (!ModalView && !_parsed)
            {
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.SettingsModalView.bsml"), parentTransform.gameObject, host);
                ModalView.name = "NyaSettingsModal";
                ModalView.SetField("_animateParentCanvas", true);
                ApiDropDownTransform.Find("DropdownTableView").GetComponent<ModalView>().SetField("_animateParentCanvas", false);
                SfwDropDownTransform.Find("DropdownTableView").GetComponent<ModalView>().SetField("_animateParentCanvas", false);
                NsfwDropDownTransform.Find("DropdownTableView").GetComponent<ModalView>().SetField("_animateParentCanvas", false);
                
                _parsed = true;
            }
        }

        protected void ShowModal(Transform parentTransform, object host)
        {
            BaseParse(parentTransform, host);
            ParserParams.EmitEvent("close-modal");
            ParserParams.EmitEvent("open-modal");

            NyaTab.gameObject.SetActive(true);
            ApiTab.gameObject.SetActive(false);
            ScreenTab.gameObject.SetActive(false);
            MoreSettingsTab.gameObject.SetActive(false);
            TabSelector.Refresh();

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(NsfwFeatures)));
        }

        public void HideModal()
        {
            if (ModalView != null)
            {
                ModalView.Hide(false);
                _nsfwConfirmModalController.HideModal();
            }
        }

        #region NyaTab

        #region Values

        [UIValue("nya-nsfw-check")]
        protected bool NsfwCheck
        {
            get => PluginConfig.NsfwImages;
            set
            {
                PluginConfig.NsfwImages = value;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(NsfwCheck)));
            }
        }

        #endregion

        #region Actions

        [UIAction("nya-download-clicked")]
        protected void DownloadNya()
        {
            _uiUtils.ButtonUnderlineClick(NyaDownloadButton.gameObject);
            Task.Run(() => _imageUtils.DownloadNyaImage());
        }

        [UIAction("nya-open-clicked")]
        protected void OpenNya()
        {
            _uiUtils.ButtonUnderlineClick(NyaOpenButton.gameObject);
            Process.Start(Path.Combine(UnityGame.UserDataPath, nameof(Nya)));
        }

        [UIAction("nya-nsfw-changed")]
        protected void NsfwToggle(bool value)
        {
            if (value && !PluginConfig.SkipNsfw)
            {
                _nsfwConfirmModalController.ShowModal(NsfwCheckbox.transform, NsfwConfirmYes, NsfwConfirmNo);
            }
            else
            {
                NsfwCheck = value;
                PluginConfig.Changed();
            }
        }

        #endregion

        #endregion

        #region SourcesTab

        #region Values

        [UIValue("api-list")] 
        protected List<object> APIList = new List<object>();

        [UIValue("api-value")]
        protected string APIValue
        {
            get => PluginConfig.SelectedAPI;
            set => PluginConfig.SelectedAPI = value;
        }

        [UIValue("sfw-list")]
        protected List<object> SfwList = new List<object>();

        [UIValue("sfw-value")]
        protected string SfwValue
        {
            get => PluginConfig.SelectedEndpoints[APIValue].SelectedSfwEndpoint;
            set => PluginConfig.SelectedEndpoints[APIValue].SelectedSfwEndpoint = value;
        }

        [UIValue("nsfw-list")]
        protected List<object> NsfwList = new List<object>();

        [UIValue("nsfw-value")]
        protected string NsfwValue
        {
            get => CheckNsfwListHasEndpoints() ? PluginConfig.SelectedEndpoints[APIValue].SelectedNsfwEndpoint : "Empty";
            set => PluginConfig.SelectedEndpoints[APIValue].SelectedNsfwEndpoint = value;
        }

        #endregion

        #region Actions

        [UIAction("api-change")]
        protected void ApiChange(string value)
        {
            APIValue = value;
            UpdateLists();
        }

        [UIAction("sfw-change")]
        protected void SfwChange(string value)
        {
            PluginConfig.SelectedEndpoints[APIValue].SelectedSfwEndpoint = value;
            PluginConfig.Changed();
        }

        [UIAction("nsfw-change")]
        protected void NsfwChange(string value)
        {
            PluginConfig.SelectedEndpoints[APIValue].SelectedNsfwEndpoint = value;
            PluginConfig.Changed();
        }

        [UIAction("format-source")]
        protected string FormatSource(string value)
        {
            return value.Split('/').Last().Replace("_", " ");
        }

        #endregion

        #endregion

        #region ScreenTab

        #region Values

        #endregion

        #region Actions

        [UIAction("face-headset-clicked")]
        protected void FaceHeadsetClicked()
        {
            _uiUtils.ButtonUnderlineClick(FaceHeadsetButton.gameObject);
            _floatingScreenUtils.TweenToHeadset(_mainCamera.camera);
        }

        [UIAction("set-upright-clicked")]
        protected void SetUprightClicked()
        {
            _uiUtils.ButtonUnderlineClick(SetUprightButton.gameObject);
            _floatingScreenUtils.TweenUpright();
        }

        [UIAction("default-position-clicked")]
        protected void DefaultPositionClicked()
        {
            _uiUtils.ButtonUnderlineClick(DefaultPositionButton.gameObject);
            _floatingScreenUtils.TweenToDefaultPosition();
        }

        [UIAction("show-handle-changed")]
        protected void ShowHandleChanged(bool value)
        {
            if (_handle == null)
                _handle = RootTransform.root.GetChild(1).gameObject;

            _timeTweeningManager.KillAllTweens(_handle);
            Vector3Tween tween;
            if (value)
            {
                _handle.gameObject.transform.localScale = new Vector3(0f, _floatingScreenUtils.HandleScale.y, _floatingScreenUtils.HandleScale.z);
                var oldScale = _handle.transform.localScale;
                tween = new Vector3Tween(oldScale, _floatingScreenUtils.HandleScale, val => _handle.transform.localScale = val, 0.5f, EaseType.OutQuart)
                {
                    onStart = delegate { _handle.gameObject.SetActive(true); }
                };
            }
            else
            {
                var newScale = new Vector3(0f, _floatingScreenUtils.HandleScale.y, _floatingScreenUtils.HandleScale.z);
                tween = new Vector3Tween(_floatingScreenUtils.HandleScale, newScale, val => _handle.transform.localScale = val, 0.5f, EaseType.OutQuart)
                {
                    onCompleted = delegate { _handle.gameObject.SetActive(false); }
                };
            }
            
            _timeTweeningManager.AddTween(tween, _handle);
        }

        #endregion

        #endregion

        #region MoreSettingsTab

        #endregion

        private void NsfwConfirmYes()
        {
            NsfwCheck = true;
            PluginConfig.Changed();
        }

        private void NsfwConfirmNo()
        {
            if (NsfwCheck) // Stops from editing the config if the nsfw value is already false
            {
                NsfwCheck = false;
                PluginConfig.Changed();
            }
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(NsfwCheck)));
        }

        private bool CheckNsfwListHasEndpoints() // Thanks Nekos.life for removing all of your 30+ nsfw endpoints
        {
            if (ImageSources.Sources[APIValue].NsfwEndpoints.Count == 0)
            {
                NsfwDropDownListSetting.values = new List<object> {"Empty"};
                NsfwDropDownListSetting.UpdateChoices();
                NsfwDropDownListSetting.Value = "Empty";
                NsfwCheck = false;
                NsfwCheckbox.interactable = false;
                NsfwDropDownListSetting.interactable = false;
                return false;
            }

            return true;
        }

        private void UpdateLists()
        {
            SfwDropDownListSetting.values.Clear();
            SfwDropDownListSetting.values = ImageSources.Sources[APIValue].SfwEndpoints.Cast<object>().ToList();
            SfwDropDownListSetting.UpdateChoices();
            SfwDropDownListSetting.Value = SfwValue;
            
            if (CheckNsfwListHasEndpoints())
            {
                if (NsfwDropDownListSetting.interactable == false)
                {
                    NsfwCheckbox.interactable = true;
                    NsfwDropDownListSetting.interactable = true;
                }
                
                NsfwDropDownListSetting.values.Clear();
                NsfwDropDownListSetting.values = ImageSources.Sources[APIValue].NsfwEndpoints.Cast<object>().ToList();
                NsfwDropDownListSetting.Value = NsfwValue;
                NsfwDropDownListSetting.UpdateChoices();
            }
        }

        public void Initialize()
        {
            APIList = ImageSources.Sources.Keys.Cast<object>().ToList();
            SfwList = ImageSources.Sources[APIValue].SfwEndpoints.Cast<object>().ToList();
            NsfwList = NsfwList = ImageSources.Sources[APIValue].NsfwEndpoints.Cast<object>().ToList();
        }
    }
}