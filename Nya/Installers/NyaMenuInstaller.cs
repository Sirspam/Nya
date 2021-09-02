using Nya.UI.ViewControllers;
using Zenject;

namespace Nya.Installers
{
    class NyaMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<NyaModifierController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsModalController>().AsSingle();
            Container.BindInterfacesAndSelfTo<NSFWConfirmModalController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsViewController>().AsSingle();
        }
    }
}
