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
	[Plugin(RuntimeOptions.DynamicInit)]
	[NoEnableDisable]
	public class Plugin
	{
		[Init]
		public Plugin(Config conf, Logger logger, Zenjector zenjector)
		{
			zenjector.UseLogger(logger);
			zenjector.UseHttpService();
			zenjector.UseMetadataBinder<Plugin>();
			zenjector.UseSiraSync();

			var config = conf.Generated<PluginConfig>();

			zenjector.Install<NyaAppInstaller>(Location.App, config);
			zenjector.Install<NyaMenuInstaller>(Location.Menu);
			zenjector.Install<NyaGameInstaller>(Location.Singleplayer);

			var folderPath = Path.Combine(UnityGame.UserDataPath, "Nya");
			Directory.CreateDirectory(Path.Combine(folderPath, "sfw"));
			Directory.CreateDirectory(Path.Combine(folderPath, "nsfw"));

			if (!config.RememberNsfw)
			{
				config.Nsfw = false;
			}
		}
	}
}