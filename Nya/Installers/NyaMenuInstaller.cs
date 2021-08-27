using Nya.UI.ViewControllers;
using Zenject;

namespace Nya.Installers
{
    class NyaMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NyaModifierController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsModalController>().AsSingle();
            Container.BindInterfacesTo<SettingsViewController>().AsSingle();
        }
    }
}
