using Nya.Configuration;
using Nya.UI.ViewControllers;
using Nya.Utils;
using Zenject;

namespace Nya.Installers
{
    internal class NyaGameInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (PluginConfig.Instance.inPause)
            {
                Container.BindInterfacesAndSelfTo<NyaViewGameController>().AsSingle();
                Container.BindInterfacesAndSelfTo<SettingsModalController>().AsSingle();
                Container.BindInterfacesAndSelfTo<NSFWConfirmModalController>().AsSingle();

                Container.BindInterfacesAndSelfTo<UIUtils>().AsSingle();
            }
        }
    }
}
