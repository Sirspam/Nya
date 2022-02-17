using HMUI;
using Zenject;

namespace Nya.UI.ViewControllers
{
    internal class NyaSettingsFlowCoordinator : FlowCoordinator
    {
        private SettingsViewMainPanelController _settingsViewMainPanelController = null!;
        private SettingsViewRightPanelController _settingsViewRightPanelController = null!;

        [Inject]
        public void Constructor(SettingsViewMainPanelController settingsViewMainPanelController, SettingsViewRightPanelController settingsViewRightPanelController)
        {
            _settingsViewMainPanelController = settingsViewMainPanelController;
            _settingsViewRightPanelController = settingsViewRightPanelController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            ProvideInitialViewControllers(_settingsViewMainPanelController, rightScreenViewController: _settingsViewRightPanelController);
        }
    }
}