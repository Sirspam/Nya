using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using Nya.Configuration;
using Nya.Utils;
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
        private PluginConfig _config = null!;
        private TimeTweeningManager _uwuTweenyManager = null!;
        private UIUtils _uiUtils = null!;

        private bool _visible;

        [Inject]
        public void Constructor(PluginConfig config, UIUtils uiUtils, TimeTweeningManager timeTweeningManager)
        {
            _config = config;
            _uiUtils = uiUtils;
            _uwuTweenyManager = timeTweeningManager;
        }

        [UIComponent("rainbow-text")]
        private readonly TMP_Text _rainbowText = null!;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_config.RainbowBackgroundColor)
            {
                _rainbowText.SetText("<#FF0000>R<#FF7F00>A<#FFFF00>I<#00FF00>N<#0000FF>B<#4B0082>O<#9400D3>W\n<#FFFFFF>Enabled");
                KindlyAskRainbowTextToShowUpThenHaveItSodOffAfterFourSeconds();
                _uiUtils.RainbowNyaBg(true);
                _config.RainbowBackgroundColor = true;
            }
            else
            {
                _rainbowText.SetText("<#FF0000>R<#FF7F00>A<#FFFF00>I<#00FF00>N<#0000FF>B<#4B0082>O<#9400D3>W\n<#FFFFFF>Disabled");
                KindlyAskRainbowTextToShowUpThenHaveItSodOffAfterFourSeconds();
                _uiUtils.RainbowNyaBg(false);
                _config.RainbowBackgroundColor = false;
            }
        }

        private void KindlyAskRainbowTextToShowUpThenHaveItSodOffAfterFourSeconds() // Code maintainability is important!
        {
            if (_visible) return;
            _uwuTweenyManager.KillAllTweens(_rainbowText);
            var tween = new FloatTween(0, 1f, val => _rainbowText.alpha = val, 0.5f, EaseType.InOutQuad)
            {
                onCompleted = delegate
                {
                    _visible = true;
                    Task.Delay(4000);
                    _uwuTweenyManager.KillAllTweens(_rainbowText);
                    var tween = new FloatTween(1, 0f, val => _rainbowText.alpha = val, 0.5f, EaseType.InOutQuad)
                    {
                        onCompleted = delegate { _visible = false; }
                    };
                    _uwuTweenyManager.AddTween(tween, _rainbowText);
                }
            };
            _uwuTweenyManager.AddTween(tween, _rainbowText);
        }
    }
}