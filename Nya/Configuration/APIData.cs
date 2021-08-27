
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using System.Collections.Generic;


namespace Nya.Configuration
{
    class APIData
    {
        public virtual string URL { get; set; }
        public virtual string selectedSFW_Endpoint { get; set; }
        public virtual string selectedNSFW_Endpoint { get; set; }
        [NonNullable, UseConverter(typeof(ListConverter<string>))]
        public virtual List<string> SFW_Endpoints { get; set; }
        [NonNullable, UseConverter(typeof(ListConverter<string>))]
        public virtual List<string> NSFW_Endpoints { get; set; }
    }
}
