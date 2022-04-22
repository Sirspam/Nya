using HMUI;
using Nya.Configuration;
using Nya.UI.ViewControllers;
using Nya.UI.ViewControllers.SettingsControllers;
using Zenject;

namespace Nya.UI.FlowControllers
{
    internal class NyaSettingsFlowCoordinator : FlowCoordinator
    {
        private NyaSettingsMainViewController _nyaSettingsMainViewController = null!;
        private NyaSettingsRightViewController _nyaSettingsRightViewController = null!;

        [Inject]
        public void Constructor(NyaSettingsMainViewController nyaSettingsMainViewController, NyaSettingsRightViewController nyaSettingsRightViewController)
        {
            _nyaSettingsMainViewController = nyaSettingsMainViewController;
            _nyaSettingsRightViewController = nyaSettingsRightViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            ProvideInitialViewControllers(_nyaSettingsMainViewController, rightScreenViewController: _nyaSettingsRightViewController);
        }
    }
}