using Nya.UI.ViewControllers;
using Nya.Utils;
using Zenject;

namespace Nya.Installers
{
    internal class NyaMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<NyaViewMenuController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsModalMenuController>().AsSingle();
            Container.BindInterfacesAndSelfTo<NsfwConfirmModalController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsViewMainPanelController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsViewRightPanelController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<NyaSettingsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();

            Container.BindInterfacesAndSelfTo<UIUtils>().AsSingle();
        }
    }
}