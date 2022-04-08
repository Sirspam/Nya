using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
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
        public virtual bool SeparatePositions { get; set; } = false;
        public virtual Vector3 MenuPosition { get; set; } = new Vector3(0f, 3.65f, 4f);
        public virtual Vector3 MenuRotation { get; set; } = new Vector3(335f, 0f, 0f);
        public virtual Vector3 PausePosition { get; set; } = new Vector3(0f, 3.65f, 4f);
        public virtual Vector3 PauseRotation { get; set; } = new Vector3(335f, 0f, 0f);
        public virtual bool UseBackgroundColor { get; set; } = false;
        public virtual Color BackgroundColor { get; set; } = new Color(0.745f, 0.745f, 0.745f);
        public virtual bool RainbowBackgroundColor { get; set; } = false;
        public virtual int AutoNyaWait { get; set; } = 4;
        public virtual int ScaleRatio { get; set; } = 512; // 0 means scaling disabled
        public virtual bool EasterEggs { get; set; } = true;
        public virtual string SelectedAPI { get; set; } = ImageSources.Sources.Keys.First();

        [NonNullable][UseConverter(typeof(DictionaryConverter<EndpointData>))]
        public virtual Dictionary<string, EndpointData> SelectedEndpoints { get; set; } = new Dictionary<string, EndpointData>();
        
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

        /// <summary>
        /// Some magic stuff to save config changes to disk deferred
        /// </summary>
        public virtual IDisposable ChangeTransaction => null!;

        /// <remark>
        /// May have to make this check more than just the count in the future but for now this works
        /// Let's pray that the user never dare tampers with the config otherwise values in the SelectedEndpoints will never fix themselves
        /// </remark>
        private void FixConfigIssues()
        {
            if (AutoNyaWait < 4)
            {
                AutoNyaWait = 4;
            }
            
            if (SelectedEndpoints.Count == ImageSources.Sources.Count)
            {
                return;
            }

            using var _ = ChangeTransaction;
            SelectedEndpoints.Clear();
            foreach (var key in ImageSources.Sources.Keys)
            {
                SelectedEndpoints.Add(key, new EndpointData
                {
                    SelectedSfwEndpoint = ImageSources.Sources[key].SfwEndpoints[0],
                    SelectedNsfwEndpoint = ImageSources.Sources[key].NsfwEndpoints[0]
                });
            }
        }
    }
}