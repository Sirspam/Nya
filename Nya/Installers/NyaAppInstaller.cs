using Nya.Configuration;
using Nya.Utils;
using Zenject;

namespace Nya.Installers
{
    internal class NyaAppInstaller : Installer
    {
        private readonly PluginConfig config;

        public NyaAppInstaller(PluginConfig config)
        {
            this.config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(config).AsSingle();
            Container.Bind<ImageUtils>().AsSingle();
            Container.Bind<UIUtils>().AsSingle();
        }
    }
}