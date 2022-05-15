using Nya.Configuration;
using Nya.UI.ViewControllers.ModalControllers;
using Nya.UI.ViewControllers.NyaViewControllers;
using Zenject;

namespace Nya.Installers
{
    internal class NyaGameInstaller : Installer
    {
        private readonly PluginConfig _config;

        public NyaGameInstaller(PluginConfig config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            if (!_config.InPause)
            {
                return;
            }

            Container.BindInterfacesTo<NyaViewGameController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsModalGameController>().AsSingle();
            Container.Bind<NsfwConfirmModalController>().AsSingle();
        }
    }
}