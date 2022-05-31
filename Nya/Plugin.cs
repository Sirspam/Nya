using System.IO;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Logging;
using IPA.Utilities;
using Nya.Configuration;
using Nya.Installers;
using SiraUtil.Zenject;

namespace Nya
{
    [Plugin(RuntimeOptions.DynamicInit)][NoEnableDisable]
    public class Plugin
    {
        [Init]
        public Plugin(Config config, Logger logger, Zenjector zenjector)
        {
            zenjector.UseLogger(logger);
            zenjector.UseHttpService();
            zenjector.UseMetadataBinder<Plugin>();
            zenjector.UseSiraSync();
            
            var pluginConfig = config.Generated<PluginConfig>();

            if (!pluginConfig.RememberNsfw || !pluginConfig.NsfwFeatures)
            {
                pluginConfig.NsfwImages = false;
            }

            zenjector.Install<NyaAppInstaller>(Location.App, pluginConfig);
            zenjector.Install<NyaMenuInstaller>(Location.Menu);
            zenjector.Install<NyaGameInstaller>(Location.Singleplayer);

            var folderPath = Path.Combine(UnityGame.UserDataPath, "Nya");
            Directory.CreateDirectory(Path.Combine(folderPath, "sfw"));
            Directory.CreateDirectory(Path.Combine(folderPath, "nsfw"));
        }
    }
}