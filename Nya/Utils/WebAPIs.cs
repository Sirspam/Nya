using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nya.Utils
{
    class WebAPIs
    {
        public static Dictionary<string, APIData> APIs { get; set; } = new Dictionary<string, APIData>() {
            { "waifu.pics", new APIData() {
                URL = "https://api.waifu.pics/",
                SFW_Endpoints = new List<string> { "sfw/neko", "sfw/waifu", "sfw/awoo", "sfw/shinobu", "sfw/megumin", "sfw/cuddle", "sfw/cry", "sfw/hug", "sfw/kiss", "sfw/lick", "sfw/pat", "sfw/smug", "sfw/bonk", "sfw/yeet", "sfw/blush", "sfw/smile", "sfw/wave", "sfw/highfive", "sfw/nom", "sfw/bite", "sfw/glomp", "sfw/slap", "sfw/kick", "sfw/happy", "sfw/wink", "sfw/poke", "sfw/dance" }, // No 'kill' endpoint because I don't like it :( 
                NSFW_Endpoints = new List<string> { "nsfw/neko", "nsfw/waifu", "nsfw/trap", "nsfw/blowjob" }
            } },
            { "nekos.life", new APIData() {
                URL = "https://nekos.life/api/v2/img/",
                SFW_Endpoints = new List<string> { "neko", "ngif", "waifu", "smug" },
                NSFW_Endpoints = new List<string> { "lewd", "lewd_gif", "yuri", "futa", "femdom", "boobs", "cum"} // Not all API endpoints because nekos.life has so many nsfw endpoints it's ludicrous
            } }
        };
    }
}
