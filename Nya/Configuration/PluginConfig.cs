using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Nya.Utils;
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace Nya.Configuration
{
    internal class PluginConfig
    {
        public bool Nsfw { get; set; } = false;
        public virtual bool RememberNsfw { get; set; } = false;
        public virtual bool SkipNsfw { get; set; } = false;
        public virtual bool InMenu { get; set; } = false;
        public virtual bool InPause { get; set; } = false;
        public virtual bool ShowHandle { get; set; } = false;
        public virtual bool SeperatePositions { get; set; } = false;
        public virtual Vector3 MenuPosition { get; set; } = new Vector3(0f, 3.65f, 4f);
        public virtual Vector3 MenuRotation { get; set; } = new Vector3(335f, 0f, 0f);
        public virtual Vector3 PausePosition { get; set; } = new Vector3(0f, 3.65f, 4f);
        public virtual Vector3 PauseRotation { get; set; } = new Vector3(335f, 0f, 0f);
        public virtual bool RainbowBackgroundColor { get; set; } = false;
        public virtual Color BackgroundColor { get; set; } = new Color(0.745f, 0.745f, 0.745f);
        public virtual int AutoNyaWait { get; set; } = 4;
        public virtual string SelectedAPI { get; set; } = WebAPIs.APIs.Keys.First();

        [NonNullable, UseConverter(typeof(DictionaryConverter<EndpointData>))]
        public virtual Dictionary<string, EndpointData> SelectedEndpoints { get; set; } = new Dictionary<string, EndpointData>();

        //{
        //    {
        //        "waifu.pics", new EndpointData
        //        {
        //            SelectedSfwEndpoint = "sfw/neko",
        //            SelectedNsfwEndpoint = "nsfw/neko"
        //        }
        //    },
        //    {
        //        "nekos.life", new EndpointData
        //        {
        //            SelectedSfwEndpoint = "neko",
        //            SelectedNsfwEndpoint = "lewd",
        //        }
        //    },
        //    {
        //        "Local Files", new EndpointData
        //        {
        //            SelectedSfwEndpoint = "/sfw",
        //            SelectedNsfwEndpoint = "/nsfw",
        //        }
        //    }
        //};

        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.
            FixConfigIssues();
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.
            FixConfigIssues();
        }

        private void FixConfigIssues()
        {
            if (SelectedEndpoints.Count == WebAPIs.APIs.Count)
            {
                return;
            }

            SelectedEndpoints.Clear();
            foreach (var key in WebAPIs.APIs.Keys)
            {
                SelectedEndpoints.Add(key, new EndpointData
                {
                    SelectedSfwEndpoint = WebAPIs.APIs[key].SfwEndpoints[0],
                    SelectedNsfwEndpoint = WebAPIs.APIs[key].NsfwEndpoints[0]
                });
            }
        }
    }
}