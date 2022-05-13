using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using IPA.Loader;
using Nya.Configuration;
using Nya.UI.ViewControllers.ModalControllers;
using Nya.Utils;
using SiraUtil.Zenject;
using TMPro;
using Tweening;
using Zenject;

namespace Nya.UI.ViewControllers.SettingsControllers
{
    [HotReload(RelativePathToLayout = @"..\..\Views\NyaSettingsRightView.bsml")]
    [ViewDefinition("Nya.UI.Views.NyaSettingsRightView.bsml")]
    internal class NyaSettingsRightViewController : BSMLAutomaticViewController
    {
        private PluginConfig _pluginConfig = null!;
        private PluginMetadata _pluginMetadata = null!;
        private FloatingScreenUtils _floatingScreenUtils = null!;
        private TimeTweeningManager _timeTweeningManager = null!;
        private GitHubPageModalController _gitHubPageModalController = null!;
        private NyaSettingsMainViewController _nyaSettingsMainViewController = null!;

        private bool _visible;

        [Inject]
        public void Constructor(PluginConfig pluginConfig, UBinder<Plugin, PluginMetadata> pluginMetadata, FloatingScreenUtils floatingScreenUtils, TimeTweeningManager timeTweeningManager, GitHubPageModalController gitHubPageModalController, NyaSettingsMainViewController nyaSettingsMainViewController)
        {
            _pluginConfig = pluginConfig;
            _pluginMetadata = pluginMetadata.Value;
            _floatingScreenUtils = floatingScreenUtils;
            _timeTweeningManager = timeTweeningManager;
            _gitHubPageModalController = gitHubPageModalController;
            _nyaSettingsMainViewController = nyaSettingsMainViewController;
        }
        
        [UIValue("version-text-value")]
        private string VersionText => $"{_pluginMetadata.Name} v{_pluginMetadata.HVersion} by {_pluginMetadata.Author}";

        [UIValue("rainbow-nya-available")]
        private bool RainbowNyaAvailable => _pluginConfig.InMenu;

        [UIComponent("rainbow-text")]
        private readonly TMP_Text _rainbowText = null!;

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);

            // Thank you leaderboard panel for kidnapping my right panel
            gameObject.SetActive(false);
        }

        [UIAction("version-text-clicked")]
        private void VersionTextClicked()
        {
            if (_pluginMetadata.PluginHomeLink == null)
            {
                return;
            }

            _gitHubPageModalController.ShowModal(_rainbowText.transform, _pluginMetadata.Name, _pluginMetadata.PluginHomeLink.ToString());
        }
        
        [UIAction("rainbow-clicked")]
        public void RainbowClicked()
        {
            if (!_pluginConfig.RainbowBackgroundColor)
            {
                _rainbowText.SetText("<#FF0000>R<#FF7F00>A<#FFFF00>I<#00FF00>N<#0000FF>B<#4B0082>O<#9400D3>W\n<#FFFFFF>Enabled");
                KindlyAskRainbowTextToShowUpThenHaveItSodOffAfterTwoSeconds();
                _floatingScreenUtils.ToggleRainbowNyaBg(true);
                _pluginConfig.RainbowBackgroundColor = true;
                _pluginConfig.BackgroundColor = _nyaSettingsMainViewController.BgColorSetting.CurrentColor;
                _nyaSettingsMainViewController.BgColorSetting.interactable = false;
                _nyaSettingsMainViewController.BgColorDefaultButton.interactable = false;
            }
            else
            {
                _rainbowText.SetText("<#FF0000>R<#FF7F00>A<#FFFF00>I<#00FF00>N<#0000FF>B<#4B0082>O<#9400D3>W\n<#FFFFFF>Disabled");
                KindlyAskRainbowTextToShowUpThenHaveItSodOffAfterTwoSeconds();
                _floatingScreenUtils.ToggleRainbowNyaBg(false);
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