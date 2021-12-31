using IPA.Utilities;
using System.Collections.Generic;
using System.IO;

namespace Nya.Utils
{
    public class WebAPIs
    {
        internal static Dictionary<string, APIData> APIs { get; } = new Dictionary<string, APIData>
        {
            {
                "waifu.pics", new APIData
                {
                    URL = "https://api.waifu.pics/",
                    json = "url",
                    SfwEndpoints = new List<string> { "sfw/neko", "sfw/waifu", "sfw/awoo", "sfw/shinobu", "sfw/megumin", "sfw/cuddle", "sfw/cry", "sfw/hug", "sfw/kiss", "sfw/lick", "sfw/pat", "sfw/smug", "sfw/bonk", "sfw/yeet", "sfw/blush", "sfw/smile", "sfw/wave", "sfw/highfive", "sfw/nom", "sfw/bite", "sfw/glomp", "sfw/slap", "sfw/kick", "sfw/happy", "sfw/wink", "sfw/poke", "sfw/dance" }, // No 'kill' endpoint because I don't like it :(
                    NsfwEndpoints = new List<string> { "nsfw/neko", "nsfw/waifu", "nsfw/trap", "nsfw/blowjob" }
                }
            },
            {
                "nekos.life", new APIData
                {
                    URL = "https://nekos.life/api/v2/img/",
                    json = "url",
                    SfwEndpoints = new List<string> { "neko", "ngif", "waifu", "smug", "gecg", "meow" },
                    NsfwEndpoints = new List<string> { "lewd", "lewd_gif", "yuri", "futa", "femdom", "boobs", "cum"} // Not all API endpoints because nekos.life has so many nsfw endpoints it's ludicrous
                }
            },
            {
                "Local Files", new APIData
                {
                    URL = Path.Combine(UnityGame.UserDataPath, "Nya"),
                    json = null,
                    SfwEndpoints = new List<string> { "/sfw" },
                    NsfwEndpoints = new List<string> { "/nsfw" }
                }
            }
        };
    }

    internal class APIData
    {
        internal string URL { get; set; }
        internal string? json { get; set; }
        internal List<string> SfwEndpoints { get; set; } = new List<string>();
        internal List<string> NsfwEndpoints { get; set; } = new List<string>();
    }
}