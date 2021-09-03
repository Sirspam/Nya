
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace Nya.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public virtual bool NSFW { get; set; } = false; // Must be 'virtual' if you want BSIPA to detect a value change and save the config automatically.
        public virtual bool rememberNSFW { get; set; } = false;
        public virtual bool skipNSFW { get; set; } = false;
        public virtual int autoNyaWait { get; set; } = 8;
        public virtual string selectedAPI { get; set; } = "waifu.pics";
        [NonNullable, UseConverter(typeof(DictionaryConverter<EndpointData>))]
        public virtual Dictionary<string, EndpointData> APIs { get; set; } = new Dictionary<string, EndpointData>() { 
            { "waifu.pics", new EndpointData() { 
                selected_SFW_Endpoint = "sfw/neko",
                selected_NSFW_Endpoint = "nsfw/neko"
            } },
            { "nekos.life", new EndpointData() { 
                selected_SFW_Endpoint = "neko",
                selected_NSFW_Endpoint = "lewd",
            } }
        };

        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
        }
    }
}
