using Nya.UI.ViewControllers;
using Zenject;

namespace Nya.Installers
{
    internal class NyaMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NyaViewMenuController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsModalMenuController>().AsSingle();
            Container.Bind<NsfwConfirmModalController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsViewMainPanelController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsViewRightPanelController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<NyaSettingsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
        }
    }
}