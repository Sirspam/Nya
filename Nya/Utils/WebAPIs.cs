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
                    BaseEndpoint = "https://api.waifu.pics/",
                    Mode = DataMode.Json,
                    SfwEndpoints = new List<string> { "sfw/neko", "sfw/waifu", "sfw/awoo", "sfw/shinobu", "sfw/megumin", "sfw/cuddle", "sfw/cry", "sfw/hug", "sfw/kiss", "sfw/lick", "sfw/pat", "sfw/smug", "sfw/bonk", "sfw/yeet", "sfw/blush", "sfw/smile", "sfw/wave", "sfw/highfive", "sfw/nom", "sfw/bite", "sfw/glomp", "sfw/slap", "sfw/kick", "sfw/happy", "sfw/wink", "sfw/poke", "sfw/dance" }, // No 'kill' endpoint because I don't like it :(
                    NsfwEndpoints = new List<string> { "nsfw/neko", "nsfw/waifu", "nsfw/trap", "nsfw/blowjob" }
                }
            },
            {
                "nekos.life", new APIData
                {
                    BaseEndpoint = "https://nekos.life/api/v2/img/",
                    Mode = DataMode.Json,
                    SfwEndpoints = new List<string> { "neko", "ngif", "waifu", "smug", "gecg", "meow" },
                    NsfwEndpoints = new List<string> { "lewd", "lewd_gif", "yuri", "futa", "femdom", "boobs", "cum"} // Not all API endpoints because nekos.life has so many nsfw endpoints it's ludicrous
                }
            },
            {
              "", new APIData
              {
                  BaseEndpoint = "https://api.xsky.dev/",
                  Mode = DataMode.Json,
                  SfwEndpoints = new List<string> { "/neko", "/catboy", "/furry", "/ff" },
                  NsfwEndpoints = new List<string> { "/hentai", "/bsdm", "/feet", "/trap", "/gif", "/futa" }
              }
            },
            {
                "Local Files", new APIData
                {
                    BaseEndpoint = Path.Combine(UnityGame.UserDataPath, "Nya"),
                    Mode = DataMode.Local,
                    SfwEndpoints = new List<string> { "/sfw" },
                    NsfwEndpoints = new List<string> { "/nsfw" }
                }
            }
        };
    }

    internal class APIData
    {
        internal string BaseEndpoint { get; set; } = string.Empty;
        internal DataMode Mode { get; set; }
        internal List<string> SfwEndpoints { get; set; } = new List<string>();
        internal List<string> NsfwEndpoints { get; set; } = new List<string>();
    }

    internal enum DataMode
    {
        Unsupported,
        Json,
        Local,
    }
}