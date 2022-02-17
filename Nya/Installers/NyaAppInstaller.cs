using Nya.AffinityPatches;
using Nya.Configuration;
using Nya.Utils;
using Zenject;

namespace Nya.Installers
{
    internal class NyaAppInstaller : Installer
    {
        private readonly PluginConfig _config;

        public NyaAppInstaller(PluginConfig config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
            Container.Bind<ImageUtils>().AsSingle();
            Container.Bind<UIUtils>().AsSingle();
        }
    }
}