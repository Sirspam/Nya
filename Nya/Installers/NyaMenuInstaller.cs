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
            Container.BindInterfacesAndSelfTo<SettingsModalController>().AsSingle();
            Container.BindInterfacesAndSelfTo<NSFWConfirmModalController>().AsSingle();
            Container.BindInterfacesTo<SettingsViewController>().AsSingle();

            Container.BindInterfacesAndSelfTo<UIUtils>().AsSingle();
        }
    }
}
