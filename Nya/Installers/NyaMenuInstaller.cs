using Nya.UI.ViewControllers;
using Nya.Utils;
using SiraUtil;
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
            Container.BindInterfacesAndSelfTo<SettingsViewController>().FromNewComponentAsViewController().AsSingle();

            Container.BindInterfacesAndSelfTo<UIUtils>().AsSingle();
        }
    }
}