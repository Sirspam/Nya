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
        public virtual bool DoneEnableNsfwFeaturesSteps { get; set; } = false;
        public virtual bool NsfwFeatures { get; set; } = false;
        public bool NsfwImages { get; set; } = false;
        public virtual bool RememberNsfw { get; set; } = false;
        public virtual bool SkipNsfw { get; set; } = false;
        public virtual bool InMenu { get; set; } = false;
        public virtual bool InPause { get; set; } = false;
        public virtual float FloatingScreenScale { get; set; } = 1f;
        public virtual bool ShowHandle { get; set; } = false;
        public virtual bool SeparatePositions { get; set; } = false;
        public virtual Vector3 MenuPosition { get; set; } = FloatingScreenUtils.DefaultPosition;
        public virtual Vector3 MenuRotation { get; set; } = FloatingScreenUtils.DefaultRotation.eulerAngles;
        public virtual Vector3 PausePosition { get; set; } = FloatingScreenUtils.DefaultPosition;
        public virtual Vector3 PauseRotation { get; set; } = FloatingScreenUtils.DefaultRotation.eulerAngles;
        public virtual bool UseBackgroundColor { get; set; } = false;
        public virtual Color BackgroundColor { get; set; } = new Color(0.745f, 0.745f, 0.745f);
        public virtual bool RainbowBackgroundColor { get; set; } = false;
        public virtual bool PersistantAutoNya { get; set; } = false;
        public virtual int AutoNyaWait { get; set; } = 5;
        public virtual int ImageScaleValue { get; set; } = 512; // 0 means scaling disabled
        public virtual bool EasterEggs { get; set; } = true;
        public virtual string SelectedAPI { get; set; } = ImageSources.Sources.Keys.First();

        [NonNullable][UseConverter(typeof(DictionaryConverter<EndpointData>))]
        public virtual Dictionary<string, EndpointData> SelectedEndpoints { get; set; } = new Dictionary<string, EndpointData>();
        
        /// <summary>
        /// Some magic stuff to save config changes to disk deferred
        /// </summary>
        public virtual IDisposable ChangeTransaction => null!;

        // Surely this innocent looking boolean won't be used for anything silly or goofy :clueless:
        public virtual bool IsAprilFirst => DateTime.Now is {Month: 4, Day: 1} && EasterEggs;
        
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
            // Stops any changes to the config happening until this method is done
            using var _ = ChangeTransaction;
            
            // Checks auto nya's wait time hasn't been set to below 3 seconds
            // Stops users from spamming the fuck out of an API
            if (AutoNyaWait < 3)
            {
                AutoNyaWait = 3;
            }
            
            // Checks if the currently selected API is supported
            if (!ImageSources.Sources.ContainsKey(SelectedAPI))
            {
                SelectedEndpoints.Remove(SelectedAPI);
                SelectedAPI = ImageSources.Sources.First().Key;
            }

            // Adds a supported API to the selected endpoints dictionary
            if (!SelectedEndpoints.ContainsKey(SelectedAPI))
            {
                SelectedEndpoints.Add(SelectedAPI, new EndpointData());
            }

            // Checks the sfw endpoint on the selected API exists
            var selectedSfwEndpoint = SelectedEndpoints[SelectedAPI].SelectedSfwEndpoint;
            if (!ImageSources.GlobalEndpoints.Contains(selectedSfwEndpoint) && !ImageSources.Sources[SelectedAPI].SfwEndpoints.Contains(selectedSfwEndpoint))
            {
                SelectedEndpoints[SelectedAPI].SelectedSfwEndpoint = ImageSources.Sources[SelectedAPI].SfwEndpoints.FirstOrDefault() ?? "Empty";
            }
            
            // Checks the nsfw endpoint on the selected API exists
            var selectedNsfwEndpoint = SelectedEndpoints[SelectedAPI].SelectedNsfwEndpoint;
            if (!ImageSources.GlobalEndpoints.Contains(selectedNsfwEndpoint) && !ImageSources.Sources[SelectedAPI].SfwEndpoints.Contains(selectedSfwEndpoint))
            {
                SelectedEndpoints[SelectedAPI].SelectedNsfwEndpoint = ImageSources.Sources[SelectedAPI].NsfwEndpoints.FirstOrDefault() ?? "Empty";
            }
        }
    }
}