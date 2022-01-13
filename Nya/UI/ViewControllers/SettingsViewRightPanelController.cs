using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using Nya.Configuration;
using Nya.Utils;
using System.Threading.Tasks;
using TMPro;
using Tweening;
using UnityEngine.EventSystems;
using Zenject;

namespace Nya.UI.ViewControllers
{
    [HotReload(RelativePathToLayout = @"..\Views\SettingsViewRightPanel.bsml")]
    [ViewDefinition("Nya.UI.Views.SettingsViewRightPanel.bsml")]
    public class SettingsViewRightPanelController : BSMLAutomaticViewController, IPointerClickHandler
    {
        private TimeTweeningManager uwuTweenyManager;
        private UIUtils uiUtils;

        private bool visible = false;

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
                KindlyAskRainbowTextToShowUpThenHaveItSodOffAfterFourSeconds();
                uiUtils.RainbowNyaBG(true);
                PluginConfig.Instance.RainbowBackgroundColor = true;
            }
            else
            {
                rainbowText.SetText("<#FF0000>R<#FF7F00>A<#FFFF00>I<#00FF00>N<#0000FF>B<#4B0082>O<#9400D3>W\n<#FFFFFF>Disabled");
                KindlyAskRainbowTextToShowUpThenHaveItSodOffAfterFourSeconds();
                uiUtils.RainbowNyaBG(false);
                PluginConfig.Instance.RainbowBackgroundColor = false;
            }
        }

        private void KindlyAskRainbowTextToShowUpThenHaveItSodOffAfterFourSeconds() // Code maintainability is important!
        {
            if (visible) return;
            uwuTweenyManager.KillAllTweens(rainbowText);
            FloatTween tween = new FloatTween(0, 1f, val => rainbowText.alpha = val, 0.5f, EaseType.InOutQuad)
            {
                onCompleted = async delegate ()
                {
                    visible = true;
                    await Task.Delay(4000);
                    uwuTweenyManager.KillAllTweens(rainbowText);
                    FloatTween tween = new FloatTween(1, 0f, val => rainbowText.alpha = val, 0.5f, EaseType.InOutQuad)
                    {
                        onCompleted = delegate () { visible = false; }
                    };
                    uwuTweenyManager.AddTween(tween, rainbowText);
                }
            };
            uwuTweenyManager.AddTween(tween, rainbowText);
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);

            // Thank you leaderboard panel for kidnapping my right panel
            gameObject.SetActive(false);
        }
    }
}