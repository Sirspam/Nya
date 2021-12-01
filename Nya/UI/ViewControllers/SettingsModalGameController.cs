using Nya.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nya.UI.ViewControllers
{
    public class SettingsModalGameController : SettingsModalController
    {
        public SettingsModalGameController(NsfwConfirmModalController nsfwConfirmModalController, SettingsViewController settingsViewController, UIUtils uiUtils) : base(nsfwConfirmModalController, settingsViewController, uiUtils)
        {
        }
    }
}
