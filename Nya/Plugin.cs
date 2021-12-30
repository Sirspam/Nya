using System.IO;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Logging;
using IPA.Utilities;
using Nya.Installers;
using Nya.Utils;
using SiraUtil.Zenject;

namespace Nya
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        [Init]
        public Plugin(Config conf, Logger logger, Zenjector zenjector)
        {
            zenjector.UseLogger(logger);
            zenjector.UseHttpService();
            
            var config = conf.Generated<Configuration.PluginConfig>();

            zenjector.Install<NyaAppInstaller>(Location.App, config);
            zenjector.Install<NyaMenuInstaller>(Location.Menu);
            zenjector.Install<NyaGameInstaller>(Location.Singleplayer);

            var folderPath = Path.Combine(UnityGame.UserDataPath, "Nya");
            Directory.CreateDirectory(Path.Combine(folderPath, "sfw"));
            Directory.CreateDirectory(Path.Combine(folderPath, "nsfw"));

            // TODO: Move logic to config itself
            if (!config.RememberNsfw)
                config.Nsfw = false;

            // May have to make this check more than just the count in the future but for now this works
            // Let's pray that the user never dare tampers with the config otherwise values in the SelectedEndpoints will never fix themselves
            // enums? I hardly know thems!
            if (config.SelectedEndpoints.Count != WebAPIs.APIs.Count)
            {
                config.SelectedEndpoints.Clear();
                foreach (string key in WebAPIs.APIs.Keys)
                {
                    config.SelectedEndpoints.Add(key, new Configuration.EndpointData()
                    {
                        SelectedSfwEndpoint = WebAPIs.APIs[key].SfwEndpoints[0],
                        SelectedNsfwEndpoint = WebAPIs.APIs[key].NsfwEndpoints[0]
                    });
                }
            }
        }
    }
}