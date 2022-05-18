using System.Collections.Generic;
using System.IO;
using IPA.Utilities;

namespace Nya.Utils
{
    public class ImageSources
    {
        internal static Dictionary<string, SourceData> Sources { get; } = new Dictionary<string, SourceData>
        {
            {
                "waifu.pics", new SourceData
                {
                    BaseEndpoint = "https://api.waifu.pics/",
                    Mode = DataMode.Json,
                    SfwEndpoints = new List<string>
                    {
                        "sfw/neko", "sfw/waifu", "sfw/awoo", "sfw/shinobu", "sfw/megumin", "sfw/cuddle", "sfw/cry",
                        "sfw/hug", "sfw/kiss", "sfw/lick", "sfw/pat", "sfw/smug", "sfw/bonk", "sfw/yeet", "sfw/blush",
                        "sfw/smile", "sfw/wave", "sfw/highfive", "sfw/nom", "sfw/bite", "sfw/glomp", "sfw/slap",
                        "sfw/kick", "sfw/happy", "sfw/wink", "sfw/poke", "sfw/dance"
                    }, // No 'kill' endpoint because it makes me sad :(
                    NsfwEndpoints = new List<string> { "nsfw/neko", "nsfw/waifu", "nsfw/trap", "nsfw/blowjob" }
                }
            },
            {
                "nekos.life", new SourceData
                {
                    BaseEndpoint = "https://nekos.life/api/v2/img/",
                    Mode = DataMode.Json,
                    SfwEndpoints = new List<string>
                    {
                        "neko", "waifu", "tickle", "slap", "pat", "meow", "lizard", "kiss", "hug", "fox_girl",
                        "feed", "cuddle", "ngif", "smug", "woof", "wallpaper", "goose", "gecg", "avatar"
                    },
                    NsfwEndpoints = new List<string>()
                }
            },
            {
                "xSky", new SourceData
                {
                    BaseEndpoint = "https://api.xsky.dev/",
                    Mode = DataMode.Json,
                    SfwEndpoints = new List<string> { "neko", "catboy" },
                    NsfwEndpoints = new List<string> { "hentai", "bdsm", "furry", "ff", "feet", "trap", "gif", "futa" }
                }
            },
            {
                "Anime-Images API", new SourceData
                {
                    BaseEndpoint = "https://anime-api.hisoka17.repl.co/img/",
                    Mode = DataMode.Json,
                    SfwEndpoints = new List<string> { "hug", "kiss", "slap", "wink", "pat", "kill", "cuddle", "punch", "waifu" },
                    NsfwEndpoints = new List<string> { "hentai", "boobs", "lesbian" }
                }
            },
            {
                "Local Files", new SourceData
                {
                    BaseEndpoint = Path.Combine(UnityGame.UserDataPath, "Nya"),
                    Mode = DataMode.Local,
                    SfwEndpoints = new List<string> { "/sfw" },
                    NsfwEndpoints = new List<string> { "/nsfw" }
                }
            }
        };

        internal struct SourceData
        {
            internal string BaseEndpoint;
            internal DataMode Mode;
            internal List<string> SfwEndpoints;
            internal List<string> NsfwEndpoints;
        }
    }

    internal enum DataMode
    {
        Unsupported,
        Json,
        Local
    }
}