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
    internal class SettingsViewRightPanelController : BSMLAutomaticViewController, IPointerClickHandler
    {
        private PluginConfig _config;
        private TimeTweeningManager _uwuTweenyManager;
        private UIUtils _uiUtils;

        private bool visible = false;

        [Inject]
        public void Constructor(PluginConfig config, UIUtils uiUtils, TimeTweeningManager timeTweeningManager)
        {
            _config = config;
            _uiUtils = uiUtils;
            _uwuTweenyManager = timeTweeningManager;
        }

        [UIComponent("rainbow-text")]
        private readonly TMP_Text rainbowText;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_config.RainbowBackgroundColor)
            {
                rainbowText.SetText("<#FF0000>R<#FF7F00>A<#FFFF00>I<#00FF00>N<#0000FF>B<#4B0082>O<#9400D3>W\n<#FFFFFF>Enabled");
                KindlyAskRainbowTextToShowUpThenHaveItSodOffAfterFourSeconds();
                _uiUtils.RainbowNyaBG(true);
                _config.RainbowBackgroundColor = true;
            }
            else
            {
                rainbowText.SetText("<#FF0000>R<#FF7F00>A<#FFFF00>I<#00FF00>N<#0000FF>B<#4B0082>O<#9400D3>W\n<#FFFFFF>Disabled");
                KindlyAskRainbowTextToShowUpThenHaveItSodOffAfterFourSeconds();
                _uiUtils.RainbowNyaBG(false);
                _config.RainbowBackgroundColor = false;
            }
        }

        private void KindlyAskRainbowTextToShowUpThenHaveItSodOffAfterFourSeconds() // Code maintainability is important!
        {
            if (visible) return;
            _uwuTweenyManager.KillAllTweens(rainbowText);
            FloatTween tween = new FloatTween(0, 1f, val => rainbowText.alpha = val, 0.5f, EaseType.InOutQuad)
            {
                onCompleted = async delegate
                {
                    visible = true;
                    await Task.Delay(4000);
                    _uwuTweenyManager.KillAllTweens(rainbowText);
                    FloatTween tween = new FloatTween(1, 0f, val => rainbowText.alpha = val, 0.5f, EaseType.InOutQuad)
                    {
                        onCompleted = delegate { visible = false; }
                    };
                    _uwuTweenyManager.AddTween(tween, rainbowText);
                }
            };
            _uwuTweenyManager.AddTween(tween, rainbowText);
        }
    }
}