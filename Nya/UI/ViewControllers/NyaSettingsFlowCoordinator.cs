using HMUI;
using Zenject;

namespace Nya.UI.ViewControllers
{
    public class NyaSettingsFlowCoordinator : FlowCoordinator
    {
        private SettingsViewMainPanelController settingsViewMainPanelController;
        private SettingsViewRightPanelController settingsViewRightPanelController;

        [Inject]
        public void Constructor(SettingsViewMainPanelController settingsViewMainPanelController, SettingsViewRightPanelController settingsViewRightPanelController)
        {
            this.settingsViewMainPanelController = settingsViewMainPanelController;
            this.settingsViewRightPanelController = settingsViewRightPanelController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            ProvideInitialViewControllers(settingsViewMainPanelController, rightScreenViewController: settingsViewRightPanelController);
        }
    }
}