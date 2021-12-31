using Nya.Configuration;
using Nya.UI.ViewControllers;
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

            Container.BindInterfacesAndSelfTo<NyaViewGameController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsModalGameController>().AsSingle();
            Container.BindInterfacesAndSelfTo<NsfwConfirmModalController>().AsSingle();
        }
    }
}