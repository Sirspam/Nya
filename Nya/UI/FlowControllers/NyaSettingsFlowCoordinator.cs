using HMUI;
using Nya.Configuration;
using Nya.UI.ViewControllers;
using Nya.UI.ViewControllers.SettingsControllers;
using Zenject;

namespace Nya.UI.FlowControllers
{
    internal class NyaSettingsFlowCoordinator : FlowCoordinator
    {
        private PluginConfig _pluginConfig = null!;
        private NyaSettingsMainViewController _nyaSettingsMainViewController = null!;
        private NyaSettingsRightViewController _nyaSettingsRightViewController = null!;

        [Inject]
        public void Constructor(PluginConfig pluginConfig, NyaSettingsMainViewController nyaSettingsMainViewController, NyaSettingsRightViewController nyaSettingsRightViewController)
        {
            _pluginConfig = pluginConfig;
            _nyaSettingsMainViewController = nyaSettingsMainViewController;
            _nyaSettingsRightViewController = nyaSettingsRightViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (_pluginConfig.InMenu)
            {
                ProvideInitialViewControllers(_nyaSettingsMainViewController, rightScreenViewController: _nyaSettingsRightViewController);   
            }
            else
            {
                ProvideInitialViewControllers(_nyaSettingsMainViewController);
            }
        }
    }
}