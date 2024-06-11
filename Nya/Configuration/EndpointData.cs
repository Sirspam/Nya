using System.Collections.Generic;

namespace Nya.Configuration
{
    internal class EndpointData
    {
        internal string? SelectedSfwEndpoint { get; set; }
        internal string? SelectedNsfwEndpoint { get; set; }

        // Dictionary to store Fluxpoint SFW endpoints
        public static readonly Dictionary<string, string> FluxpointSfwEndpoints = new Dictionary<string, string>
        {
            {"anime", "sfw/img/anime"},
            {"azurlane", "sfw/img/azurlane"},
            {"chibi", "sfw/img/chibi"},
            {"christmas", "sfw/img/christmas"},
            {"halloween", "sfw/img/halloween"},
            {"holo", "sfw/img/holo"},
            {"kitsune", "sfw/img/kitsune"},
            {"maid", "sfw/img/maid"},
            {"neko", "sfw/img/neko"},
            {"nekoboy", "sfw/img/nekoboy"},
            {"nekopara", "sfw/img/nekopara"},
            {"senko", "sfw/img/senko"},
            {"wallpaper", "sfw/img/wallpaper"},
            
        // SFW gif endpoints
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
        };

        // Dictionary to store Fluxpoint NSFW endpoints
        public static readonly Dictionary<string, string> FluxpointNsfwEndpoints = new Dictionary<string, string>
        {
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
            {"futa", "nsfw/img/futa"},
            {"gasm", "nsfw/img/gasm"},
            {"holo", "nsfw/img/holo"},
            {"lewd", "nsfw/img/lewd"},
            {"neko", "nsfw/img/neko"},
            {"nekopara", "nsfw/img/nekopara"},
            {"pantyhose", "nsfw/img/pantyhose"},
            {"peeing", "nsfw/img/peeing"},
            {"petplay", "nsfw/img/petplay"},
            {"pussy", "nsfw/img/pussy"},
            {"slimes", "nsfw/img/slimes"},
            {"solo", "nsfw/img/solo"},
            {"trap", "nsfw/img/trap"},
            {"yaoi", "nsfw/img/yaoi"},
            {"yuri", "nsfw/img/yuri"},
        
        // NSFW gif endpoints
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
            {"kuni", "nsfw/gif/kuni"},
            {"neko gif", "nsfw/gif/neko"},
            {"pussy gif", "nsfw/gif/pussy"},
            {"wank", "nsfw/gif/wank"},
            {"solo gif", "nsfw/gif/solo"},
            {"spank", "nsfw/gif/spank"},
            {"tentacle gif", "nsfw/gif/tentacle"},
            {"toys", "nsfw/gif/toys"},
            {"yuri gif", "nsfw/gif/yuri"},
        };

        // Method to retrieve the endpoint based on category for SFW
        public static string GetFluxpointSfwEndpoint(string category)
        {
            if (FluxpointSfwEndpoints.ContainsKey(category))
            {
                return FluxpointSfwEndpoints[category];
            }
            return null;
        }

        // Method to retrieve the endpoint based on category for NSFW
        public static string GetFluxpointNsfwEndpoint(string category)
        {
            if (FluxpointNsfwEndpoints.ContainsKey(category))
            {
                return FluxpointNsfwEndpoints[category];
            }
            return null;
        }
    }
}
