using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using IPA.Utilities;
using Nya.Utils;
using Nya.Configuration;
using Zenject;
using Tweening;
using System.Threading.Tasks;

namespace Nya.UI.ViewControllers
{
    [HotReload(RelativePathToLayout = @"..\Views\SettingsViewRightPanel.bsml")]
    [ViewDefinition("Nya.UI.Views.SettingsViewRightPanel.bsml")]
    public class SettingsViewRightPanelController : BSMLAutomaticViewController, IPointerClickHandler
    {
        private TimeTweeningManager uwuTweenyManager;
        private UIUtils uiUtils;

        [Inject]
        public void Constructor(TimeTweeningManager timeTweeningManager, UIUtils uiUtils)
        {
            this.uwuTweenyManager = timeTweeningManager;
            this.uiUtils = uiUtils;
        }

        [UIComponent("rainbow-text")]
        private readonly TMP_Text rainbowText;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!PluginConfig.Instance.RainbowBackgroundColor)
            {
                rainbowText.SetText("<#FF0000>R<#FF7F00>A<#FFFF00>I<#00FF00>N<#0000FF>B<#4B0082>O<#9400D3>W\n<#FFFFFF>Enabled");
                KindlyAskRainbowTextToShowUpThenHaveItSodOffAfter4Seconds();
                uiUtils.RainbowNyaBG(true);
                PluginConfig.Instance.RainbowBackgroundColor = true;
            }
            else
            {
                rainbowText.SetText("<#FF0000>R<#FF7F00>A<#FFFF00>I<#00FF00>N<#0000FF>B<#4B0082>O<#9400D3>W\n<#FFFFFF>Disabled");
                KindlyAskRainbowTextToShowUpThenHaveItSodOffAfter4Seconds();
                uiUtils.RainbowNyaBG(false);
                PluginConfig.Instance.RainbowBackgroundColor = false;
            }
        }

        private void KindlyAskRainbowTextToShowUpThenHaveItSodOffAfter4Seconds()
        {
            uwuTweenyManager.KillAllTweens(rainbowText);
            FloatTween tween = new FloatTween(0, 1f, val => rainbowText.alpha = val, 0.5f, EaseType.InOutQuad)
            {
                onCompleted = async delegate ()
                {
                    await Task.Delay(4000);
                    FloatTween tween = new FloatTween(1, 0f, val => rainbowText.alpha = val, 0.5f, EaseType.InOutQuad);
                    uwuTweenyManager.AddTween(tween, rainbowText);
                }
            };
            uwuTweenyManager.AddTween(tween, rainbowText);
        }
    }
}