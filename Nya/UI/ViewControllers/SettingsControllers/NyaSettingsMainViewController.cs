using System.Collections.Generic;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Loader;
using IPA.Utilities;
using Nya.Components;
using Nya.Configuration;
using Nya.UI.ViewControllers.ModalControllers;
using Nya.Utils;
using SiraUtil.Logging;
using SiraUtil.Web.SiraSync;
using SiraUtil.Zenject;
using TMPro;
using Tweening;
using UnityEngine;
using UnityEngine.UI;
using VRUIControls;
using Zenject;

namespace Nya.UI.ViewControllers.SettingsControllers
{
    [HotReload(RelativePathToLayout = @"..\..\Views\NyaSettingsMainView.bsml")]
    [ViewDefinition("Nya.UI.Views.NyaSettingsMainView.bsml")]
    internal class NyaSettingsMainViewController : BSMLAutomaticViewController
    {
        public FlowCoordinator parentFlowCoordinator = null!;

        private bool _updateAvailable;
        private bool _inMenu;
        private bool _inPause;
        private float _floatingScreenScale;
        private Color _backgroundColor;
        private int _autoNyaWait;
        private bool _persistantAutoNya;
        private int _scaleValue;
        private bool _separatePositions;
        private bool _easterEggs;
        private bool _nsfwFeatures;
        private bool _rememberNsfw;
        private bool _skipNsfw;
        
        private bool _useBackgroundColor;
        private Vector3 _menuPosition;
        private Vector3 _menuRotation;
        private Vector3 _pausePosition;
        private Vector3 _pauseRotation;

        private SiraLog _siraLog = null!;
        private UIUtils _uiUtils = null!;
        private PluginConfig _pluginConfig = null!;
        private PluginMetadata _pluginMetadata = null!;
        private ISiraSyncService _siraSyncService = null!;
        private FloatingScreenUtils _floatingScreenUtils = null!;
        private TimeTweeningManager _timeTweeningManager = null!;
        private MainFlowCoordinator _mainFlowCoordinator = null!;
        private MenuTransitionsHelper _menuTransitionsHelper = null!;
        private EnableNsfwFeaturesModalController _enableNsfwFeaturesModalController = null!;

        [Inject]
        public void Constructor(SiraLog siraLog, UIUtils uiUtils, PluginConfig pluginConfig, UBinder<Plugin, PluginMetadata> pluginMetadata, ISiraSyncService siraSyncService, FloatingScreenUtils floatingScreenUtils, TimeTweeningManager timeTweeningManager, MainFlowCoordinator mainFlowCoordinator, MenuTransitionsHelper menuTransitionsHelper, EnableNsfwFeaturesModalController enableNsfwFeaturesModalController)
        {
            _siraLog = siraLog;
            _uiUtils = uiUtils;
            _pluginConfig = pluginConfig;
            _pluginMetadata = pluginMetadata.Value;
            _siraSyncService = siraSyncService;
            _floatingScreenUtils = floatingScreenUtils;
            _timeTweeningManager = timeTweeningManager;
            _mainFlowCoordinator = mainFlowCoordinator;
            _menuTransitionsHelper = menuTransitionsHelper;
            _enableNsfwFeaturesModalController = enableNsfwFeaturesModalController;
        }

        protected override async void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            
            if (firstActivation)
            {
                _topPanel.gameObject.AddComponent<VRGraphicRaycaster>().SetField("_physicsRaycaster", BeatSaberUI.PhysicsRaycasterWithCache);
                _topPanel.gameObject.GetComponent<Canvas>().additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
                _chocolaImage.name = "ChocolaImage";
                _vanillaImage.name = "VanillaImage";
                _chocolaImage.gameObject.AddComponent<NyaSettingsClickableImage>();
                _vanillaImage.gameObject.AddComponent<NyaSettingsClickableImage>();

                BgColorSetting.modalColorPicker.doneEvent += dontCareDidntAsk => _pluginConfig.UseBackgroundColor = true;
                BgColorSetting.modalColorPicker.cancelEvent += BgColorSettingCancelled;
                            
                var gitVersion = await _siraSyncService.LatestVersion();
                if (gitVersion != null && gitVersion > _pluginMetadata.HVersion)
                {
                    _siraLog.Info($"{nameof(Nya)} v{gitVersion} is available on GitHub!");
                    _updateText.text = $"{nameof(Nya)} v{gitVersion} is available on GitHub!";
                    _updateText.alpha = 0f;
                    UpdateAvailable = true;
                    _timeTweeningManager.AddTween(new FloatTween(0f, 1f, val => _updateText.alpha = val, 0.4f, EaseType.InCubic), this);
                }
            }
            else
            {
                AssignValues();
            }

            if (_pluginConfig.RainbowBackgroundColor)
            {
                BgColorSetting.interactable = false;
                BgColorDefaultButton.interactable = false;
            }
        }

        [UIValue("update-available")]
        private bool UpdateAvailable
        {
            get => _updateAvailable;
            set
            {
                _updateAvailable = value;
                NotifyPropertyChanged();
            }
        }
        
        [UIValue("restart-required")]
        private bool RestartRequired => InMenu != _pluginConfig.InMenu && isActiveAndEnabled;

        [UIValue("in-menu")]
        private bool InMenu
        {
            get => _inMenu;
            set
            {
                _inMenu = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(RestartRequired));
            }
        }

        [UIValue("in-pause")]
        private bool InPause
        {
            get => _inPause;
            set
            {
                _inPause = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("floating-screen-scale-value")]
        private float FloatingScreenScale
        {
            get => _floatingScreenScale;
            set
            {
                _floatingScreenScale = value;
                NotifyPropertyChanged();
            }
        }
        
        [UIValue("bg-color")]
        private Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                NotifyPropertyChanged();
            }

        }

        [UIValue("auto-wait-value")]
        private int AutoNyaWait
        {
            get => _autoNyaWait;
            set
            {
                _autoNyaWait = value;
                NotifyPropertyChanged();
            }
        }
        
        [UIValue("persistant-auto-nya")]
        private bool PersistantAutoNya
        {
            get => _persistantAutoNya;
            set
            {
                _persistantAutoNya = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("scaling-choices")] 
        private List<object> _scalingChoices = new List<object> { "Disabled", 128, 256, 512, 1024 };

        [UIValue("scaling-value")]
        private object ScalingValue
        {
            get
            {
                if (_scaleValue == 0)
                    return "Disabled";
                return _scaleValue;
            }
            set
            {
                if (value.ToString() == "Disabled")
                {
                    _scaleValue = 0;
                    NotifyPropertyChanged();
                    return;
                }
                
                _scaleValue = (int) value;
                NotifyPropertyChanged();

            }
        }

        [UIValue("separate-positions")]
        private bool SeparatePositions
        {
            get => _separatePositions;
            set
            {
                _separatePositions = value;
                SeparatePositionsButOpposite = !value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("separate-positions-but-opposite")] // Might be a better way to do this 💀
        private bool SeparatePositionsButOpposite
        {
            get => !SeparatePositions;
            set => NotifyPropertyChanged();
        }

        [UIValue("easter-eggs")]
        private bool EasterEggs
        {
            get => _easterEggs;
            set
            {
                _easterEggs = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("done-enable-nsfw-features-steps")]
        private bool DoneEnableNsfwFeaturesSteps
        {
            get => _pluginConfig.DoneEnableNsfwFeaturesSteps;
            set
            {
                _pluginConfig.DoneEnableNsfwFeaturesSteps = value;
                NotDoneEnableNsfwFeaturesSteps = !value;
                NotifyPropertyChanged();
            }
        }
        
        [UIValue("not-done-enable-nsfw-features-steps")]
        private bool NotDoneEnableNsfwFeaturesSteps
        {
            get => !DoneEnableNsfwFeaturesSteps;
            set => NotifyPropertyChanged();
        }
        
        [UIValue("NSFW-features")]
        private bool NsfwFeatures
        {
            get => _nsfwFeatures;
            set
            {
                _nsfwFeatures = value;
                NotifyPropertyChanged();
            }
        }
        
        [UIValue("remember-NSFW")]
        private bool RememberNsfw
        {
            get => _rememberNsfw;
            set
            {
                _rememberNsfw = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("skip-NSFW")]
        private bool SkipNsfw
        {
            get => _skipNsfw;
            set
            {
                _skipNsfw = value;
                NotifyPropertyChanged();
            }
        }
        
        [UIComponent("update-text")] 
        private readonly TextMeshProUGUI _updateText = null!;
        
        [UIComponent("top-panel")] 
        private readonly HorizontalOrVerticalLayoutGroup _topPanel = null!;
        
        [UIComponent("chocola-image")]
        private readonly ImageView _chocolaImage = null!;
        
        [UIComponent("vanilla-image")]
        private readonly ImageView _vanillaImage = null!;

        [UIComponent("bg-color-setting")]
        public readonly ColorSetting BgColorSetting = null!;

        [UIComponent("bg-color-default")]
        public readonly Button BgColorDefaultButton = null!;

        [UIComponent("reset-menu-position")]
        private readonly Button _resetMenuPositionButton = null!;

        [UIComponent("reset-pause-position")]
        private readonly Button _resetPausePositionButton = null!;
        
        [UIComponent("enable-nsfw-features-button")]
        private readonly Button _enableNsfwFeaturesButton = null!;

        private void BgColorSettingCancelled()
        {
            if ((!_useBackgroundColor || !_pluginConfig.UseBackgroundColor) && _pluginConfig.InMenu && _floatingScreenUtils.MenuFloatingScreen != null)
            { 
                _floatingScreenUtils.SetStandardMaterial();
            }
        }
        
        [UIAction("#post-parse")]
        private void PostParse()
        {
            AssignValues();
        }

        [UIAction("floating-screen-scale-changed")]
        private void FloatingScreenScaleChanged(float scale)
        {
            _floatingScreenUtils.ScaleFloatingScreen(scale);
        }
        
        [UIAction("bg-color-setting-changed")]
        private void BgColorSettingChanged(Color value)
        {
            _backgroundColor = value;
            _floatingScreenUtils.SetNyaMaterialColor(value);
        }
        
        [UIAction("bg-color-default-clicked")]
        private void BgColorDefaultClicked()
        {
            _uiUtils.ButtonUnderlineClick(BgColorDefaultButton.gameObject);
            _pluginConfig.UseBackgroundColor = false;
            _floatingScreenUtils.SetStandardMaterial();
        }

        [UIAction("reset-menu-clicked")]
        private void ResetMenuPosition()
        {
            _uiUtils.ButtonUnderlineClick(_resetMenuPositionButton.gameObject);
            _menuPosition = FloatingScreenUtils.DefaultPosition;
            _menuRotation = FloatingScreenUtils.DefaultRotation.eulerAngles;
            if (_pluginConfig.InMenu && _floatingScreenUtils.MenuFloatingScreen != null)
            {
                _floatingScreenUtils.TweenToDefaultPosition(false);
            }
        }

        [UIAction("reset-pause-clicked")]
        private void ResetPausePosition()
        {
            _uiUtils.ButtonUnderlineClick(_resetPausePositionButton.gameObject);
            _pausePosition = FloatingScreenUtils.DefaultPosition;
            _pauseRotation = FloatingScreenUtils.DefaultRotation.eulerAngles;
        }

        [UIAction("enable-nsfw-features-clicked")]
        private void EnableNsfwFeaturesClicked()
        {
            _enableNsfwFeaturesModalController.ShowModal(_enableNsfwFeaturesButton.transform, () =>
            {
                DoneEnableNsfwFeaturesSteps = true;
                NsfwFeatures = true;
            });
        }
        
        [UIAction("redo-nsfw-features-steps-clicked")]
        private void RedoEnableNsfwFeaturesClicked()
        {
            _enableNsfwFeaturesModalController.ShowModal(_enableNsfwFeaturesButton.transform, null);
        }

        private void AssignValues()
        {
            InMenu = _pluginConfig.InMenu;
            InPause = _pluginConfig.InPause;
            FloatingScreenScale = _pluginConfig.FloatingScreenScale;
            BackgroundColor = _pluginConfig.BackgroundColor;
            AutoNyaWait = _pluginConfig.AutoNyaWait;
            PersistantAutoNya = _pluginConfig.PersistantAutoNya;
            ScalingValue = _pluginConfig.ImageScaleValue;
            SeparatePositions = _pluginConfig.SeparatePositions;
            EasterEggs = _pluginConfig.EasterEggs;
            NsfwFeatures = _pluginConfig.NsfwFeatures;
            RememberNsfw = _pluginConfig.RememberNsfw;
            SkipNsfw = _pluginConfig.SkipNsfw;
            _useBackgroundColor = _pluginConfig.UseBackgroundColor;
            _menuPosition = _pluginConfig.MenuPosition;
            _menuRotation = _pluginConfig.MenuRotation;
            _pausePosition = _pluginConfig.PausePosition;
            _pauseRotation = _pluginConfig.PauseRotation;
        }
        
        private void SaveValuesToConfig()
        {
            _pluginConfig.InMenu = InMenu;
            _pluginConfig.InPause = InPause;
            _pluginConfig.FloatingScreenScale = FloatingScreenScale;
            _pluginConfig.BackgroundColor = BackgroundColor;
            _pluginConfig.AutoNyaWait = AutoNyaWait;
            _pluginConfig.PersistantAutoNya = PersistantAutoNya;
            _pluginConfig.ImageScaleValue = _scaleValue;
            _pluginConfig.SeparatePositions = SeparatePositions;
            _pluginConfig.NsfwFeatures = NsfwFeatures;
            _pluginConfig.RememberNsfw = RememberNsfw;
            _pluginConfig.SkipNsfw = SkipNsfw;
            _pluginConfig.MenuPosition = _menuPosition;
            _pluginConfig.MenuRotation = _menuRotation;
            _pluginConfig.PausePosition = _pausePosition;
            _pluginConfig.PauseRotation = _pauseRotation;
        }

        [UIAction("ok-clicked")]
        private void OkClicked()
        {
            var restartRequired = RestartRequired;
            SaveValuesToConfig();

            if (restartRequired)
            {
                _menuTransitionsHelper.RestartGame();
            }
            else
            {
                parentFlowCoordinator.DismissFlowCoordinator(_mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf(), animationDirection: AnimationDirection.Vertical);
            }
        }

        [UIAction("cancel-clicked")]
        private void CancelClicked()
        {
            if (!_useBackgroundColor)
            {
                _pluginConfig.UseBackgroundColor = false;
                BgColorSettingCancelled();
            }
            else
            {
                _floatingScreenUtils.SetNyaMaterialColor(_pluginConfig.BackgroundColor);   
            }
            
            if (_pluginConfig.InMenu && _floatingScreenUtils.MenuFloatingScreen != null)
            {
                var floatingScreen = _floatingScreenUtils.MenuFloatingScreen;
                floatingScreen.transform.position = _pluginConfig.MenuPosition;
                floatingScreen.transform.rotation = Quaternion.Euler(_pluginConfig.MenuRotation);
            }
            
            _floatingScreenUtils.ScaleFloatingScreen(_pluginConfig.FloatingScreenScale);
            parentFlowCoordinator.DismissFlowCoordinator(_mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf(), animationDirection: AnimationDirection.Vertical);
        }
    }
}