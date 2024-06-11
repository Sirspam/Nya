using System.IO;
using Microsoft.Extensions.Configuration;

namespace Nya.Configuration
{
	public static class ConfigManager
	{
		private static IConfigurationRoot configuration;

		static ConfigManager()
		{
			configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.Build();
		}

		public static string GetFluxpointApiKey()
		{
			return configuration["FluxpointApiKey"];
		}
	}
}
