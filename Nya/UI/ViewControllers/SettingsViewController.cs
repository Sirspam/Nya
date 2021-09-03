using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Settings;
using Nya.Configuration;
using Nya.Utils;
using System;
using System.Collections.Generic;
using Zenject;


namespace Nya.UI.ViewControllers
{
    internal class SettingsViewController : IInitializable, IDisposable
    {   
        [UIValue("remember-NSFW")]
        public bool rememberNSFW
        {
            get => PluginConfig.Instance.rememberNSFW;
            set => PluginConfig.Instance.rememberNSFW = value;
        }

        [UIValue("skip-NSFW")]
        public bool skipNSFW
        {
            get => PluginConfig.Instance.skipNSFW;
            set => PluginConfig.Instance.skipNSFW = value;
        }

        [UIValue("auto-wait")]
        public int autoNyaWait
        {
            get => PluginConfig.Instance.autoNyaWait;
            set => PluginConfig.Instance.autoNyaWait = value;
        }

        [UIValue("api-list")]
        public List<object> apiList = new List<object>();

        [UIValue("api-value")]
        public string apiValue
        {
            get => PluginConfig.Instance.selectedAPI;
            set => PluginConfig.Instance.selectedAPI = value;
        }

        [UIValue("sfw-list")]
        public List<object> sfwList = new List<object>();

        [UIValue("sfw-value")]
        public string sfwValue
        {
            get => PluginConfig.Instance.APIs[apiValue].selected_SFW_Endpoint;
            set => PluginConfig.Instance.APIs[apiValue].selected_SFW_Endpoint = value;
        }

        [UIValue("nsfw-list")]
        public List<object> nsfwList = new List<object>();

        [UIValue("nsfw-value")]
        public string nsfwValue
        {
            get => PluginConfig.Instance.APIs[apiValue].selected_NSFW_Endpoint;
            set => PluginConfig.Instance.APIs[apiValue].selected_NSFW_Endpoint = value;
        }

        [UIComponent("sfw-dropdown")]
        private readonly DropDownListSetting sfwDropDownListSetting;

        [UIComponent("nsfw-dropdown")]
        private readonly DropDownListSetting nsfwDropDownListSetting;

        [UIAction("api-change")]
        public async void apiChange(string value)
        {
            apiValue = value;
            updateLists();
        }

        public void Initialize()
        {
            BSMLSettings.instance.AddSettingsMenu("Nya", "Nya.UI.Views.SettingsView.bsml", this);
            SetupLists();
        }
        public void Dispose() => BSMLSettings.instance?.RemoveSettingsMenu(this);
        public void SetupLists()
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
