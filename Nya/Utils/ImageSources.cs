using System.Collections.Generic;
using System.IO;
using IPA.Utilities;

namespace Nya.Utils
{
    internal sealed class ImageSources
    {
        internal static Dictionary<string, SourceData> Sources { get; } = new Dictionary<string, SourceData>
        {
            {
                "fluxpoint.dev", new SourceData
                {
                    BaseEndpoint = "https://gallery.fluxpoint.dev/api/",
                    Mode = DataMode.Json,
                    SfwEndpoints = new List<string>{
                         // Images
                 {"anime", "sfw/img/anime"},
                 {"azurlane", "sfw/img/azurlane"},
                 {"chibi", "sfw/img/chibi"},
                 {"christmas", "sfw/img/christmas"},
                 // {"ddlc", "sfw/img/ddlc"},
                 {"halloween", "sfw/img/halloween"},
                 {"holo", "sfw/img/holo"},
                 {"kitsune", "sfw/img/kitsune"},
                 {"maid", "sfw/img/maid"},
                 {"neko", "sfw/img/neko"},
                 {"nekoboy", "sfw/img/nekoboy"},
                 {"nekopara", "sfw/img/nekopara"},
                 {"senko", "sfw/img/senko"},
                 {"wallpaper", "sfw/img/wallpaper"},

                 // Gifs
                 {"baka", "sfw/gif/baka"},
                 {"bite", "sfw/gif/bite"},
                 {"blush", "sfw/gif/blush"},
                 {"cry", "sfw/gif/cry"},
                 {"dance", "sfw/gif/dance"},
                 {"feed", "sfw/gif/feed"},
                 {"fluff", "sfw/gif/fluff"},
                 {"grab", "sfw/gif/grab"},
                 {"handhold", "sfw/gif/handhold"},
                 {"highfive", "sfw/gif/highfive"},
                 {"hug", "sfw/gif/hug"},
                 {"kiss", "sfw/gif/kiss"},
                 {"lick", "sfw/gif/lick"},
                 {"neko gif", "sfw/gif/neko"},
                 {"pat", "sfw/gif/pat"},
                 {"poke", "sfw/gif/poke"},
                 {"punch", "sfw/gif/punch"},
                 {"shrug", "sfw/gif/shrug"},
                 {"slap", "sfw/gif/slap"},
                 {"smug", "sfw/gif/smug"},
                 {"stare", "sfw/gif/stare"},
                 {"tickle", "sfw/gif/tickle"},
                 {"wag", "sfw/gif/wag"},
                 {"wasted", "sfw/gif/wasted"},
                 {"wave", "sfw/gif/wave"},
                 {"wink", "sfw/gif/wink"},
             },
                    NsfwEndpoints = new List<string> { 
                        // Images 
                 {"anal", "nsfw/img/anal"},
                 {"anus", "nsfw/img/anus"},
                 {"ass", "nsfw/img/ass"},
                 {"azurlane", "nsfw/img/azurlane"},
                 {"bdsm", "nsfw/img/bdsm"},
                 {"blowjob", "nsfw/img/blowjob"},
                 {"boobs", "nsfw/img/boobs"},
                 {"cosplay", "nsfw/img/cosplay"},
                 {"cum", "nsfw/img/cum"},
                 {"feet", "nsfw/img/feet"},
                 // {"femdom", "nsfw/img/femdom"},
                 {"futa", "nsfw/img/futa"},
                 {"gasm", "nsfw/img/gasm"},
                 {"holo", "nsfw/img/holo"},
                 // {"kitsune", "nsfw/img/kitsune"},
                 {"lewd", "nsfw/img/lewd"},
                 {"neko", "nsfw/img/neko"},
                 {"nekopara", "nsfw/img/nekopara"},
                 // {"pantsu", "nsfw/img/pantsu"},
                 {"pantyhose", "nsfw/img/pantyhose"},
                 {"peeing", "nsfw/img/peeing"},
                 {"petplay", "nsfw/img/petplay"},
                 {"pussy", "nsfw/img/pussy"},
                 {"slimes", "nsfw/img/slimes"},
                 {"solo", "nsfw/img/solo"},
                 // {"swimsuit", "nsfw/img/swimsuit"},
                 // {"tentacle", "nsfw/img/tentacle"},
                 // {"thighs", "nsfw/img/thighs"},
                 {"trap", "nsfw/img/trap"},
                 {"yaoi", "nsfw/img/yaoi"},
                 {"yuri", "nsfw/img/yuri"},

                 // GIFs
                 {"anal gif", "nsfw/gif/anal"},
                 {"ass gif", "nsfw/gif/ass"},
                 {"bdsm gif", "nsfw/gif/bdsm"},
                 {"blowjob gif", "nsfw/gif/blowjob"},
                 {"boobjob", "nsfw/gif/boobjob"},
                 {"boobs gif", "nsfw/gif/boobs"},
                 {"cum gif", "nsfw/gif/cum"},
                 {"feet gif", "nsfw/gif/feet"},
                 {"futa gif", "nsfw/gif/futa"},
                 {"handjob", "nsfw/gif/handjob"},
                 {"hentai", "nsfw/gif/hentai"},
                 // {"kitsune (not implemented yet)", "nsfw/gif/kitsune"},
                 {"kuni", "nsfw/gif/kuni"},
                 {"neko gif", "nsfw/gif/neko"},
                 {"pussy gif", "nsfw/gif/pussy"},
                 {"wank", "nsfw/gif/wank"},
                 {"solo gif", "nsfw/gif/solo"},
                 {"spank", "nsfw/gif/spank"},
                 // {"femdom gif", "nsfw/gif/femdom"},
                 {"tentacle gif", "nsfw/gif/tentacle"},
                 {"toys", "nsfw/gif/toys"},
                 {"yuri gif", "nsfw/gif/yuri"}, }
                }
            },
            {
                "waifu.pics", new SourceData
                {
                    BaseEndpoint = "https://api.waifu.pics/",
                    Mode = DataMode.Json,
                    // Removed 'kill' endpoint because it makes me sad :(
                    SfwEndpoints = new List<string>
                    {
                        "sfw/neko", "sfw/waifu", "sfw/awoo", "sfw/shinobu", "sfw/megumin", "sfw/cuddle", "sfw/cry",
                        "sfw/hug", "sfw/kiss", "sfw/lick", "sfw/pat", "sfw/smug", "sfw/bonk", "sfw/yeet", "sfw/blush",
                        "sfw/smile", "sfw/wave", "sfw/highfive", "sfw/nom", "sfw/bite", "sfw/glomp", "sfw/slap",
                        "sfw/kick", "sfw/happy", "sfw/wink", "sfw/poke", "sfw/dance"
                    },
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
                "Anime-Images API", new SourceData
                {
                    BaseEndpoint = "https://anime-api.hisoka17.repl.co/img/",
                    Mode = DataMode.Json,
                    // Removed 'kill' endpoint because, once again, it makes me a bit sad :(
                    SfwEndpoints = new List<string> { "hug", "kiss", "slap", "wink", "pat", "cuddle", "punch", "waifu" },
                    NsfwEndpoints = new List<string> { "nsfw/hentai", "nsfw/boobs", "nsfw/lesbian" }
                }
            },
            {
                "Catboys", new SourceData
                {
                    BaseEndpoint = "https://api.catboys.com/",
                    Mode = DataMode.Json,
                    SfwEndpoints = new List<string>
                    {
                        "img", "baka"
                    },
                    NsfwEndpoints = new List<string>()
                }
            },
            {
                "BocchiTheAPI", new SourceData
                {
                    BaseEndpoint = "boccher.pixelboom.dev/api/",
                    Mode =  DataMode.Json,
                    SfwEndpoints = new List<string>
                    {
                        "frames", "frames?episode=1", "frames?episode=2", "frames?episode=3", "frames?episode=4",
                        "frames?episode=5", "frames?episode=6", "frames?episode=7", "frames?episode=8",
                        "frames?episode=9", "frames?episode=10", "frames?episode=11", "frames?episode=12",
                        "frames?episode=OP", "frames?episode=ED1", "frames?episode=ED2", "frames?episode=ED3"    
                    },
                    NsfwEndpoints = new List<string>()
                }
            },
            {
                "Local Files", new SourceData
                {
                    BaseEndpoint = Path.Combine(UnityGame.UserDataPath, "Nya"),
                    Mode = DataMode.Local,
                    SfwEndpoints = PopulateLocalEndpoints(false),
                    NsfwEndpoints = PopulateLocalEndpoints(true)
                }
            }
        };

        private static List<string> PopulateLocalEndpoints(bool nsfw)
        {
            var baseFolder = nsfw ? "nsfw" : "sfw";
            var endpoints = new List<string> {baseFolder};

            foreach (var folder in Directory.GetDirectories(Path.Combine(UnityGame.UserDataPath, "Nya", baseFolder)))
            {
                endpoints.Add(Path.GetFileName(folder));
            }

            return endpoints;
        }

        internal struct SourceData
        {
            internal string BaseEndpoint;
            internal DataMode Mode;
            internal List<string> SfwEndpoints;
            internal List<string> NsfwEndpoints;
        }

        // Empty shouldn't really be here but that's an issue I'll fix later :clueless:
        internal static readonly string[] GlobalEndpoints = {"Empty", "Random"};
        
        internal enum DataMode
        {
            Unsupported,
            Json,
            Local
        }
    }
}
