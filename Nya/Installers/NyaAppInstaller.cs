using Nya.Configuration;
using Nya.Utils;
using Zenject;

namespace Nya.Installers
{
    internal class NyaAppInstaller : Installer
    {
        private readonly PluginConfig _pluginConfig;

        public NyaAppInstaller(PluginConfig config)
        {
            _pluginConfig = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_pluginConfig).AsSingle();
            Container.Bind<UIUtils>().AsSingle();
            Container.Bind<ImageUtils>().AsSingle();
            Container.Bind<FloatingScreenUtils>().AsSingle();
        }
    }
}