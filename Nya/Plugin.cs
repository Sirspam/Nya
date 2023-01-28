using System;
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
        // Surely this innocent looking boolean won't be used for anything silly or goofy :clueless:
        public static bool IsAprilFirst { get; private set; }

        [Init]
        public Plugin(Config config, Logger logger, Zenjector zenjector)
        {
            var folderPath = Path.Combine(UnityGame.UserDataPath, "Nya");
            Directory.CreateDirectory(Path.Combine(folderPath, "sfw"));
            Directory.CreateDirectory(Path.Combine(folderPath, "nsfw"));
            
            zenjector.UseLogger(logger);
            zenjector.UseHttpService();
            zenjector.UseMetadataBinder<Plugin>();
            zenjector.UseSiraSync();
            
            var pluginConfig = config.Generated<PluginConfig>();

            if (!pluginConfig.RememberNsfw || !pluginConfig.NsfwFeatures)
            {
                pluginConfig.NsfwImages = false;
            }

            IsAprilFirst = DateTime.Now is {Month: 4, Day: 1} && pluginConfig.EasterEggs;
            
            zenjector.Install<NyaAppInstaller>(Location.App, pluginConfig);
            zenjector.Install<NyaMenuInstaller>(Location.Menu);
            zenjector.Install<NyaGameInstaller>(Location.Singleplayer);
        }
    }
}