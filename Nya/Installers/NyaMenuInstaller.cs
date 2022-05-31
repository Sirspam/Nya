using Nya.Configuration;
using Nya.UI.FlowCoordinators;
using Nya.UI.ViewControllers.ModalControllers;
using Nya.UI.ViewControllers.NyaViewControllers;
using Nya.UI.ViewControllers.SettingsControllers;
using Nya.Utils;
using Zenject;

namespace Nya.Installers
{
    internal class NyaMenuInstaller : Installer
    {
        private readonly PluginConfig _pluginConfig;

        public NyaMenuInstaller(PluginConfig pluginConfig)
        {
            _pluginConfig = pluginConfig;
        }

        public override void InstallBindings()
        {
            if (_pluginConfig.InMenu)
            {
                Container.BindInterfacesTo<NyaViewMenuController>().AsSingle();
            }
            else
            {
                Container.BindInterfacesAndSelfTo<NyaViewGameplaySetupController>().AsSingle();
            }

            Container.Bind<GitHubPageModalController>().AsSingle();
            Container.Bind<NsfwConfirmModalController>().AsSingle();
            Container.Bind<EnableNsfwFeaturesModalController>().AsSingle();
            Container.BindInterfacesTo<NyaModSettingsViewController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SettingsModalMenuController>().AsSingle();
            Container.Bind<NyaSettingsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesAndSelfTo<NyaSettingsMainViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<NyaSettingsRightViewController>().FromNewComponentAsViewController().AsSingle();
        }
    }
}