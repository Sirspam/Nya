using System.IO;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Logging;
using IPA.Utilities;
using Newtonsoft.Json;
using Nya.Configuration;
using Nya.Installers;
using SiraUtil.Zenject;

namespace Nya
{
    [Plugin(RuntimeOptions.DynamicInit)][NoEnableDisable]
    public class Plugin
    {
        // ImageSourcesJsonLink can also be a local path!
#if DEVIMAGESOURCE
        public static string ImageSourcesJsonLink =
            "https://raw.githubusercontent.com/Sirspam/Nya/dev/ImageSources.json";
#else
        public static string ImageSourcesJsonLink =
            "https://raw.githubusercontent.com/Sirspam/Nya/main/ImageSources.json";
#endif
        public static string CustomImageSourcesPath => Path.Combine(UnityGame.UserDataPath, "Nya", "CustomImageSources.json");
        
        [Init]
        public Plugin(Config config, Logger logger, Zenjector zenjector)
        {
            var folderPath = Path.Combine(UnityGame.UserDataPath, "Nya");
            Directory.CreateDirectory(Path.Combine(folderPath, "sfw"));
            Directory.CreateDirectory(Path.Combine(folderPath, "nsfw"));
            
            if (!File.Exists(CustomImageSourcesPath))
            {
                File.WriteAllText(CustomImageSourcesPath,
                    JsonConvert.SerializeObject(
                        new
                        {
                            _comment =
                                // TODO: Update link for when documentation is done
                                "Here you can add your own APIs to Nya! More info: https://github.com/Sirspam/Nya/blob/dev/README.md",
                            Sources = new {}
                        }, Formatting.Indented)); 
            }
            
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
        }
    }
}