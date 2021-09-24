using System.Collections.Generic;


namespace Nya.Utils
{
    internal class APIData
    {
        public string URL { get; set; }
        public List<string> SFW_Endpoints { get; set; }
        public List<string> NSFW_Endpoints { get; set; }
    }
}
