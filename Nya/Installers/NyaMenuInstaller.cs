using Nya.UI.FlowControllers;
using Nya.UI.ViewControllers.ModalControllers;
using Nya.UI.ViewControllers.NyaViewControllers;
using Nya.UI.ViewControllers.SettingsControllers;
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
            Container.Bind<GitHubPageModalController>().AsSingle();
            Container.BindInterfacesTo<NyaModSettingsViewController>().AsSingle();
            Container.Bind<NyaSettingsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesAndSelfTo<NyaSettingsMainViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<NyaSettingsRightViewController>().FromNewComponentAsViewController().AsSingle();
        }
    }
}