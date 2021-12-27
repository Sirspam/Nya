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
        internal static Logger Log { get; private set; }

        [Init]
        public Plugin(Config conf, Logger logger, Zenjector zenjector)
        {
            Log = logger;
            Log?.Debug("Logger initialized.");
            zenjector.Install<NyaMenuInstaller>(Location.Menu);
            zenjector.Install<NyaGameInstaller>(Location.Singleplayer);

            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            var folderPath = Path.Combine(UnityGame.UserDataPath, "Nya");
            Directory.CreateDirectory(Path.Combine(folderPath, "sfw"));
            Directory.CreateDirectory(Path.Combine(folderPath, "nsfw"));

            // TODO: Move logic to config itself
            if (!Configuration.PluginConfig.Instance.RememberNsfw) Configuration.PluginConfig.Instance.Nsfw = false;

            // May have to make this check more than just the count in the future but for now this works
            // Let's pray that the user never dare tampers with the config otherwise values in the SelectedEndpoints will never fix themselves
            // enums? I hardly know thems!
            if (Configuration.PluginConfig.Instance.SelectedEndpoints.Count != WebAPIs.APIs.Count)
            {
                Configuration.PluginConfig.Instance.SelectedEndpoints.Clear();
                foreach (string key in WebAPIs.APIs.Keys)
                {
                    Configuration.PluginConfig.Instance.SelectedEndpoints.Add(key, new Configuration.EndpointData()
                    {
                        SelectedSfwEndpoint = WebAPIs.APIs[key].SfwEndpoints[0],
                        SelectedNsfwEndpoint = WebAPIs.APIs[key].NsfwEndpoints[0]
                    });
                }
            }
            Log?.Debug("Config loaded");
        }
    }
}