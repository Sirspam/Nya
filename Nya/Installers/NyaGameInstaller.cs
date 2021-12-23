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
            if (PluginConfig.Instance.InPause)
            {
                Container.BindInterfacesAndSelfTo<NyaViewGameController>().AsSingle();
                Container.BindInterfacesAndSelfTo<SettingsModalGameController>().AsSingle();
                Container.BindInterfacesAndSelfTo<NsfwConfirmModalController>().AsSingle();

                Container.BindInterfacesAndSelfTo<UIUtils>().AsSingle();
            }
        }
    }
}