using Nya.Utils;
using Zenject;

namespace Nya.Installers
{
    internal class NyaAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<UIUtils>().AsSingle();
        }
    }
}