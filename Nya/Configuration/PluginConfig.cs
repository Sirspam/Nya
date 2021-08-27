
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
        public virtual string selectedAPI { get; set; } = "waifu.pics";
        [NonNullable, UseConverter(typeof(DictionaryConverter<APIData>))]
        public virtual Dictionary<string, APIData> APIs { get; set; } = new Dictionary<string, APIData>() { 
            { "waifu.pics", new APIData() { 
                URL = "https://api.waifu.pics/",
                selectedSFW_Endpoint = "sfw/neko",
                selectedNSFW_Endpoint = "nsfw/neko",
                SFW_Endpoints = new List<string> { "sfw/neko", "sfw/waifu", "sfw/awoo", "sfw/shinobu", "sfw/megumin", "sfw/cuddle", "sfw/cry", "sfw/hug", "sfw/kiss", "sfw/lick", "sfw/pat", "sfw/smug", "sfw/bonk", "sfw/yeet", "sfw/blush", "sfw/smile", "sfw/wave", "sfw/highfive", "sfw/nom", "sfw/bite", "sfw/glomp", "sfw/slap", "sfw/kick", "sfw/happy", "sfw/wink", "sfw/poke", "sfw/dance",  }, 
                NSFW_Endpoints = new List<string> { "nsfw/neko", "nsfw/waifu", "nsfw/trap", "nsfw/blowjob" } 
            } },
            { "nekos.life", new APIData() { 
                URL = "https://nekos.life/api/v2/img/",
                selectedSFW_Endpoint = "neko",
                selectedNSFW_Endpoint = "lewd",
                SFW_Endpoints = new List<string> { "neko", "ngif", "waifu", "smug" }, 
                NSFW_Endpoints = new List<string> { "lewd" } 
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
