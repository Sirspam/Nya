using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using Nya.Configuration;
using Nya.Utils;
using TMPro;
using Tweening;
using Zenject;

namespace Nya.UI.ViewControllers
{
    [HotReload(RelativePathToLayout = @"..\Views\NyaSettingsRightView.bsml")]
    [ViewDefinition("Nya.UI.Views.NyaSettingsRightView.bsml")]
    internal class NyaSettingsRightViewController : BSMLAutomaticViewController
    {
        private UIUtils _uiUtils = null!;
        private PluginConfig _pluginConfig = null!;
        private TimeTweeningManager _timeTweeningManager = null!;
        private NyaSettingsMainViewController _nyaSettingsMainViewController = null!;

        private bool _visible;

        [Inject]
        public void Constructor(UIUtils uiUtils, PluginConfig pluginConfig, TimeTweeningManager timeTweeningManager, NyaSettingsMainViewController nyaSettingsMainViewController)
        {
            _uiUtils = uiUtils;
            _pluginConfig = pluginConfig;
            _timeTweeningManager = timeTweeningManager;
            _nyaSettingsMainViewController = nyaSettingsMainViewController;
        }

        [UIComponent("rainbow-text")]
        private readonly TMP_Text _rainbowText = null!;

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);

            // Thank you leaderboard panel for kidnapping my right panel
            gameObject.SetActive(false);
        }
        
        [UIAction("rainbow-clicked")]
        public void RainbowClicked()
        {
            if (!_pluginConfig.RainbowBackgroundColor)
            {
                _rainbowText.SetText("<#FF0000>R<#FF7F00>A<#FFFF00>I<#00FF00>N<#0000FF>B<#4B0082>O<#9400D3>W\n<#FFFFFF>Enabled");
                KindlyAskRainbowTextToShowUpThenHaveItSodOffAfterTwoSeconds();
                _uiUtils.ToggleRainbowNyaBg(true);
                _pluginConfig.RainbowBackgroundColor = true;
                _pluginConfig.BackgroundColor = _nyaSettingsMainViewController.BgColorSetting.CurrentColor;
                _nyaSettingsMainViewController.BgColorSetting.interactable = false;
                _nyaSettingsMainViewController.BgColorDefaultButton.interactable = false;
            }
            else
            {
                _rainbowText.SetText("<#FF0000>R<#FF7F00>A<#FFFF00>I<#00FF00>N<#0000FF>B<#4B0082>O<#9400D3>W\n<#FFFFFF>Disabled");
                KindlyAskRainbowTextToShowUpThenHaveItSodOffAfterTwoSeconds();
                _uiUtils.ToggleRainbowNyaBg(false);
                _pluginConfig.RainbowBackgroundColor = false;
                _nyaSettingsMainViewController.BgColorSetting.interactable = true;
                _nyaSettingsMainViewController.BgColorDefaultButton.interactable = true;
            }
        }

        private void KindlyAskRainbowTextToShowUpThenHaveItSodOffAfterTwoSeconds() // Code maintainability is important!
        {
            if (_visible) return;
            _timeTweeningManager.KillAllTweens(_rainbowText);
            var tween = new FloatTween(0, 1f, val => _rainbowText.alpha = val, 0.5f, EaseType.InOutQuad)
            {
                onCompleted = delegate
                {
                    _visible = true;
                    var tween = new FloatTween(1, 0f, val => _rainbowText.alpha = val, 0.5f, EaseType.InOutQuad, 2f)
                    {
                        onCompleted = delegate { _visible = false; }
                    };
                    _timeTweeningManager.AddTween(tween, _rainbowText);
                }
            };
            _timeTweeningManager.AddTween(tween, _rainbowText);
        }
    }
}