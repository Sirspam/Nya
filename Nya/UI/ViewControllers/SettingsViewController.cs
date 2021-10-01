using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Settings;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


namespace Nya.UI.ViewControllers
{
    internal class SettingsViewController : IInitializable, IDisposable
    {
        private readonly UIUtils _uiUtils;

        private SettingsViewController(UIUtils uiUtils)
        {
            _uiUtils = uiUtils;
        }

        [UIValue("remember-NSFW")]
        private bool rememberNSFW
        {
            get => PluginConfig.Instance.rememberNSFW;
            set => PluginConfig.Instance.rememberNSFW = value;
        }

        [UIValue("skip-NSFW")]
        private bool skipNSFW
        {
            get => PluginConfig.Instance.skipNSFW;
            set => PluginConfig.Instance.skipNSFW = value;
        }

        [UIValue("in-menu")]
        private bool inMenu
        {
            get => PluginConfig.Instance.inMenu;
            set => PluginConfig.Instance.inMenu = value;
        }

        [UIValue("in-pause")]
        private bool inPause
        {
            get => PluginConfig.Instance.inPause;
            set => PluginConfig.Instance.inPause = value;
        }

        [UIValue("show-handle")]
        private bool pauseHandle
        {
            get => PluginConfig.Instance.showHandle;
            set => PluginConfig.Instance.showHandle = value;
        }

        [UIValue("auto-wait-value")]
        private int autoNyaWait
        {
            get => PluginConfig.Instance.autoNyaWait;
            set => PluginConfig.Instance.autoNyaWait = value;
        }

        [UIValue("api-list")]
        private List<object> apiList = new List<object>();

        [UIValue("api-value")]
        private string apiValue
        {
            get => PluginConfig.Instance.selectedAPI;
            set => PluginConfig.Instance.selectedAPI = value;
        }

        [UIValue("sfw-list")]
        private List<object> sfwList = new List<object>();

        [UIValue("sfw-value")]
        private string sfwValue
        {
            get => PluginConfig.Instance.APIs[apiValue].selected_SFW_Endpoint;
            set => PluginConfig.Instance.APIs[apiValue].selected_SFW_Endpoint = value;
        }

        [UIValue("nsfw-list")]
        private List<object> nsfwList = new List<object>();

        [UIValue("nsfw-value")]
        private string nsfwValue
        {
            get => PluginConfig.Instance.APIs[apiValue].selected_NSFW_Endpoint;
            set => PluginConfig.Instance.APIs[apiValue].selected_NSFW_Endpoint = value;
        }

        [UIComponent("reset-menu-position")]
        private readonly UnityEngine.UI.Button resetMenuPositionButton;

        [UIComponent("reset-pause-position")]
        private readonly UnityEngine.UI.Button resetPausePositionButton;

        [UIComponent("sfw-dropdown")]
        private readonly DropDownListSetting sfwDropDownListSetting;

        [UIComponent("nsfw-dropdown")]
        private readonly DropDownListSetting nsfwDropDownListSetting;

        [UIAction("api-change")]
        private async void ApiChange(string value)
        {
            apiValue = value;
            updateLists();
        }

        [UIAction("in-menu-changed")]
        private async void InMenuChanged(bool value)
        {
            if (value)
            {

            }
        }

        [UIAction("reset-menu-clicked")]
        private async void ResetMenuPosition()
        {
            _uiUtils.ButtonUnderlineClick(resetMenuPositionButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
            PluginConfig.Instance.menuPosition = new Vector3(0f, 3.65f, 4f);
            PluginConfig.Instance.menuRotation = new Vector3(340f, 0f, 0f);
        }

        [UIAction("reset-pause-clicked")]
        private async void ResetPausePosition()
        {
            _uiUtils.ButtonUnderlineClick(resetPausePositionButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
            PluginConfig.Instance.pausePosition = new Vector3(-2f, 1.5f, 0f);
            PluginConfig.Instance.pauseRotation = new Vector3(0f, 270f, 0f);
        }

        public void Initialize()
        {
            BSMLSettings.instance.AddSettingsMenu("Nya", "Nya.UI.Views.SettingsView.bsml", this);
            SetupLists();
        }
        public void Dispose() => BSMLSettings.instance?.RemoveSettingsMenu(this);

        private void SetupLists()
        {
            foreach (var api in WebAPIs.APIs.Keys)
            {
                apiList.Add(api);
            }
            foreach (var endpoint in WebAPIs.APIs[apiValue].SFW_Endpoints)
            {
                sfwList.Add(endpoint);
            }
            foreach (var endpoint in WebAPIs.APIs[apiValue].NSFW_Endpoints)
            {
                nsfwList.Add(endpoint);
            }
        }
        private void updateLists()
        {
            sfwDropDownListSetting.values.Clear();
            nsfwDropDownListSetting.values.Clear();
            foreach (var endpoint in WebAPIs.APIs[apiValue].SFW_Endpoints)
            {
                sfwDropDownListSetting.values.Add(endpoint);
            }
            foreach (var endpoint in WebAPIs.APIs[apiValue].NSFW_Endpoints)
            {
                nsfwDropDownListSetting.values.Add(endpoint);
            }
            sfwDropDownListSetting.UpdateChoices();
            nsfwDropDownListSetting.UpdateChoices();
        }
    }
}
