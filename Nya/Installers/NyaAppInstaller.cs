using Nya.AffinityPatches;
using Nya.Configuration;
using Nya.Managers;
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
            Container.Bind<ImageSourcesManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<NyaImageManager>().AsSingle();
            Container.Bind<UIUtils>().AsSingle();
            Container.Bind<WebUtils>().AsSingle();
            Container.Bind<ImageUtils>().AsSingle();
            Container.Bind<FloatingScreenUtils>().AsSingle();
            if (_pluginConfig.IsAprilFirst)
            {
                Container.BindInterfacesTo<GoodBoyPatch>().AsSingle();
            }
        }
    }
}