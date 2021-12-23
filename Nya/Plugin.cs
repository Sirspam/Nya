using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Utilities;
using Nya.Installers;
using Nya.Utils;
using SiraUtil.Zenject;
using System.IO;
using IPALogger = IPA.Logging.Logger;

namespace Nya
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        // TODO: If using Harmony, uncomment and change YourGitHub to the name of your GitHub account, or use the form "com.company.project.product"
        //       You must also add a reference to the Harmony assembly in the Libs folder.
        // public const string HarmonyId = "com.github.YourGitHub.Nya";
        // internal static readonly HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(HarmonyId);

        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Instance = this;
            Plugin.Log = logger;
            Plugin.Log?.Debug("Logger initialized.");
            zenjector.Install<NyaMenuInstaller>(Location.Menu);
            zenjector.Install<NyaGameInstaller>(Location.Singleplayer);
        }

        #region BSIPA Config

        //Uncomment to use BSIPA's config

        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            var folderPath = Path.Combine(UnityGame.UserDataPath, "Nya");
            if (!Directory.Exists($"{folderPath}/sfw")) Directory.CreateDirectory($"{folderPath}/sfw");
            if (!Directory.Exists($"{folderPath}/nsfw")) Directory.CreateDirectory($"{folderPath}/nsfw");

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
            Plugin.Log?.Debug("Config loaded");
        }

        #endregion BSIPA Config

        #region Disableable

        /// <summary>
        /// Called when the plugin is enabled (including when the game starts if the plugin is enabled).
        /// </summary>
        [OnEnable]
        public void OnEnable()
        {
            //ApplyHarmonyPatches();
        }

        /// <summary>
        /// Called when the plugin is disabled and on Beat Saber quit. It is important to clean up any Harmony patches, GameObjects, and Monobehaviours here.
        /// The game should be left in a state as if the plugin was never started.
        /// Methods marked [OnDisable] must return void or Task.
        /// </summary>
        [OnDisable]
        public void OnDisable()
        {
            //RemoveHarmonyPatches();
        }

        /*
        /// <summary>
        /// Called when the plugin is disabled and on Beat Saber quit.
        /// Return Task for when the plugin needs to do some long-running, asynchronous work to disable.
        /// [OnDisable] methods that return Task are called after all [OnDisable] methods that return void.
        /// </summary>
        [OnDisable]
        public async Task OnDisableAsync()
        {
            await LongRunningUnloadTask().ConfigureAwait(false);
        }
        */

        #endregion Disableable

        // Uncomment the methods in this section if using Harmony

        #region Harmony

        /*
        /// <summary>
        /// Attempts to apply all the Harmony patches in this assembly.
        /// </summary>
        internal static void ApplyHarmonyPatches()
        {
            try
            {
                Plugin.Log?.Debug("Applying Harmony patches.");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error("Error applying Harmony patches: " + ex.Message);
                Plugin.Log?.Debug(ex);
            }
        }

        /// <summary>
        /// Attempts to remove all the Harmony patches that used our HarmonyId.
        /// </summary>
        internal static void RemoveHarmonyPatches()
        {
            try
            {
                // Removes all patches with this HarmonyId
                harmony.UnpatchAll(HarmonyId);
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error("Error removing Harmony patches: " + ex.Message);
                Plugin.Log?.Debug(ex);
            }
        }
        */

        #endregion Harmony
    }
}