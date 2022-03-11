using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Loader;
using Nya.CatCore;
using Nya.Components;
using Nya.Configuration;
using Nya.Utils;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Nya.UI.ViewControllers
{
    [HotReload(RelativePathToLayout = @"..\Views\NyaSettingsMainView.bsml")]
    [ViewDefinition("Nya.UI.Views.NyaSettingsMainView.bsml")]
    internal class NyaSettingsMainViewController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        public FlowCoordinator parentFlowCoordinator = null!;

        private bool _inMenu;
        private bool _inPause;
        private Color _backgroundColor;
        private bool _rememberNsfw;
        private bool _skipNsfw;
        private int _autoNyaWait;
        private int _scaleRatio;
        private bool _separatePositions;
        private bool _easterEggs;
        private bool _catCoreEnabled;
        private bool _nyaCommandEnabled;
        private int _nyaCommandCooldown;
        private bool _currentNyaCommandEnabled;
        private Vector3 _menuPosition;
        private Vector3 _menuRotation;
        private Vector3 _pausePosition;
        private Vector3 _pauseRotation;

        private SiraLog _siraLog = null!;
        private UIUtils _uiUtils = null!;
        private PluginConfig _pluginConfig = null!;
        private CatCoreManager? _catCoreManager = null;
        private MainFlowCoordinator _mainFlowCoordinator = null!;
        private MenuTransitionsHelper _menuTransitionsHelper = null!;
        private NyaSettingsRightViewController _nyaSettingsRightViewController = null!;

        [Inject]
        public void Constructor(SiraLog siraLog, UIUtils uiUtils, PluginConfig pluginConfig, [InjectOptional] CatCoreManager catCoreManager, MainFlowCoordinator mainFlowCoordinator, MenuTransitionsHelper menuTransitionsHelper, NyaSettingsRightViewController nyaSettingsRightViewController)
        {
            _siraLog = siraLog;
            _uiUtils = uiUtils;
            _pluginConfig = pluginConfig;
            _catCoreManager = catCoreManager;
            _mainFlowCoordinator = mainFlowCoordinator;
            _menuTransitionsHelper = menuTransitionsHelper;
            _nyaSettingsRightViewController = nyaSettingsRightViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

            if (firstActivation)
            {
                if (PluginManager.GetPluginFromId("CatCore") == null)
                {
                    _catCoreTab.IsVisible = false;
                }
                
                _chocolaImage.name = "ChocolaImage";
                _vanillaImage.name = "VanillaImage";
                _chocolaImage.gameObject.AddComponent<NyaSettingsClickableImage>();
                _vanillaImage.gameObject.AddComponent<NyaSettingsClickableImage>();   
            }

            InMenu = _pluginConfig.InMenu;
            InPause = _pluginConfig.InPause;
            BackgroundColor = _pluginConfig.BackgroundColor;
            RememberNsfw = _pluginConfig.RememberNsfw;
            SkipNsfw = _pluginConfig.SkipNsfw;
            ScalingValue = _pluginConfig.ScaleRatio;
            SeparatePositions = _pluginConfig.SeparatePositions;
            EasterEggs = _pluginConfig.EasterEggs;
            CatCoreEnabled = _pluginConfig.CatCoreEnabled;
            NyaCommandEnabled = _pluginConfig.NyaCommandEnabled;
            NyaCommandCooldown = _pluginConfig.NyaCommandCooldown;
            CurrentNyaCommandEnabled = _pluginConfig.CurrentNyaCommandEnabled;
            _menuPosition = _pluginConfig.MenuPosition;
            _menuRotation = _pluginConfig.MenuRotation;
            _pausePosition = _pluginConfig.PausePosition;
            _pauseRotation = _pluginConfig.PauseRotation;
        }

        [UIValue("view-controller-active")]
        private bool ViewControllerActive => isActiveAndEnabled;
        
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

        [UIValue("bg-colour")]
        private Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                _uiUtils.NyaBgMaterial.color = value;
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

        [UIValue("scaling-choices")] 
        private List<object> ScalingChoices = new List<object> { "Disabled", 128, 256, 512, 1024 };

        [UIValue("scaling-value")]
        private object ScalingValue
        {
            get
            {
                if (_scaleRatio == 0)
                    return "Disabled";
                return _scaleRatio;
            }
            set
            {
                if (value.ToString() == "Disabled")
                {
                    _scaleRatio = 0;
                    NotifyPropertyChanged();
                    return;
                }
                
                _scaleRatio = (int) value;
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

        [UIValue("cat-core-enabled")]
        private bool CatCoreEnabled
        {
            get => _catCoreEnabled;
            set
            {
                _catCoreEnabled = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("nya-command-enabled")]
        private bool NyaCommandEnabled
        {
            get => _nyaCommandEnabled;
            set
            {
                _nyaCommandEnabled = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("nya-command-cooldown")]
        private int NyaCommandCooldown
        {
            get => _nyaCommandCooldown;
            set
            {
                _nyaCommandCooldown = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("current-nya-command-enabled")]
        private bool CurrentNyaCommandEnabled
        {
            get => _currentNyaCommandEnabled;
            set
            {
                _currentNyaCommandEnabled = value;
                NotifyPropertyChanged();
            }
        }

        [UIComponent("chocola-image")]
        private readonly ImageView _chocolaImage = null!;
        
        [UIComponent("vanilla-image")]
        private readonly ImageView _vanillaImage = null!;
        
        [UIComponent("cat-core-tab")]
        private readonly Tab _catCoreTab = null!;
        
        [UIComponent("bg-colour-setting")]
        private readonly Transform _bgColourSettingTransform = null!;

        [UIComponent("bg-colour-default")]
        private readonly Button _bgColourDefaultButton = null!;

        [UIComponent("reset-menu-position")]
        private readonly Button _resetMenuPositionButton = null!;

        [UIComponent("reset-pause-position")]
        private readonly Button _resetPausePositionButton = null!;
        

        [UIAction("bg-colour-default-clicked")]
        private void BgColourDefaultClicked()
        {
            _uiUtils.ButtonUnderlineClick(_bgColourDefaultButton.gameObject);
            var modalColourPicker = _bgColourSettingTransform.GetChild(2).GetComponent<ModalColorPicker>();
            modalColourPicker.CurrentColor = new Color(0.745f, 0.745f, 0.745f);
            modalColourPicker.DonePressed(); // Thank you DonePressed for making everything magically work
        }

        [UIAction("reset-menu-clicked")]
        private void ResetMenuPosition()
        {
            _uiUtils.ButtonUnderlineClick(_resetMenuPositionButton.gameObject);
            _menuPosition = new Vector3(0f, 3.65f, 4f);
            _menuRotation = new Vector3(335f, 0f, 0f);
            if (_pluginConfig.InMenu)
            {
                var floatingScreen = GameObject.Find("NyaMenuFloatingScreen");
                floatingScreen.transform.position = new Vector3(0f, 3.65f, 4f);
                floatingScreen.transform.rotation = Quaternion.Euler(new Vector3(335f, 0f, 0f));
            }
        }

        [UIAction("reset-pause-clicked")]
        private void ResetPausePosition()
        {
            _uiUtils.ButtonUnderlineClick(_resetPausePositionButton.gameObject);
            _pausePosition = new Vector3(-2f, 1.5f, 0f);
            _pauseRotation = new Vector3(0f, 270f, 0f);
        }

        private void SaveValuesToConfig()
        {
            _pluginConfig.InMenu = InMenu;
            _pluginConfig.InPause = InPause;
            _pluginConfig.BackgroundColor = BackgroundColor;
            _pluginConfig.RememberNsfw = RememberNsfw;
            _pluginConfig.SkipNsfw = SkipNsfw;
            _pluginConfig.AutoNyaWait = AutoNyaWait;
            _pluginConfig.ScaleRatio = _scaleRatio;
            _pluginConfig.SeparatePositions = SeparatePositions;
            _pluginConfig.MenuPosition = _menuPosition;
            _pluginConfig.MenuRotation = _menuRotation;
            _pluginConfig.PausePosition = _pausePosition;
            _pluginConfig.PauseRotation = _pauseRotation;
            _pluginConfig.CatCoreEnabled = CatCoreEnabled;
            _pluginConfig.NyaCommandEnabled = NyaCommandEnabled;
            _pluginConfig.NyaCommandCooldown = NyaCommandCooldown;
            _pluginConfig.CurrentNyaCommandEnabled = CurrentNyaCommandEnabled;
        }

        [UIAction("ok-clicked")]
        private void OkClicked()
        {
            var restartRequired = RestartRequired;
            var catCoreActionNeeded = _pluginConfig.CatCoreEnabled != CatCoreEnabled;
            SaveValuesToConfig();
            
            if (catCoreActionNeeded)
            {
                if (_pluginConfig.CatCoreEnabled)
                {
                    // User shouldn't even be able to access CatCore settings if it's not installed, so not going to bother making sure catCoreManager isn't null
                    _catCoreManager!.StartCatCoreServices();
                }
                else
                {
                    _catCoreManager!.EndCatCoreServices();
                }
            }
            
            if (restartRequired) 
                _menuTransitionsHelper.RestartGame();
            else 
                parentFlowCoordinator.DismissFlowCoordinator(_mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf(), animationDirection: AnimationDirection.Vertical);
        }

        [UIAction("cancel-clicked")]
        private void CancelClicked()
        {
            _uiUtils.NyaBgMaterial.color = _pluginConfig.BackgroundColor;
            if (_pluginConfig.InMenu)
            {
                var floatingScreen = GameObject.Find("NyaMenuFloatingScreen");
                floatingScreen.transform.position = _menuPosition;
                floatingScreen.transform.rotation = Quaternion.Euler(_menuRotation);
            }
            _nyaSettingsRightViewController.gameObject.SetActive(false); // Thank you leaderboard panel for kidnapping my right panel
            parentFlowCoordinator.DismissFlowCoordinator(_mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf(), animationDirection: AnimationDirection.Vertical);
        }

        [UIAction("#apply")]
        private void ModSettingsApply() => SaveValuesToConfig();
        
        [UIAction("#cancel")]
        private void ModSettingsCancel()
        {
            _uiUtils.NyaBgMaterial.color = _pluginConfig.BackgroundColor;
            if (_pluginConfig.InMenu)
            {
                var floatingScreen = GameObject.Find("NyaMenuFloatingScreen");
                floatingScreen.transform.position = _menuPosition;
                floatingScreen.transform.rotation = Quaternion.Euler(_menuRotation);
            }
        }

        public void Initialize() => BSMLSettings.instance.AddSettingsMenu("Nya", "Nya.UI.Views.NyaSettingsMainView.bsml", this);

            public void Dispose()
        {
            if (BSMLSettings.instance != null)
            {
                BSMLSettings.instance.RemoveSettingsMenu(this);
            }
        }
    }
}