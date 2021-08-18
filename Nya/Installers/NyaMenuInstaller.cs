using Nya.UI.ViewControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace Nya.Installers
{
    class NyaMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ViewController1>().AsSingle();
        }
    }
}
