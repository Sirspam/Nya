using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Nya.UI.ViewControllers
{
    [HotReload(RelativePathToLayout = @"..\Views\SettingsViewMainPanel.bsml")]
    [ViewDefinition("Nya.UI.Views.SettingsViewMainPanel.bsml")]
    internal class SettingsViewMainPanelController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        private PluginConfig _config = null!;
        private MainFlowCoordinator _mainFlowCoordinator = null!;
        private MenuTransitionsHelper _menuTransitionsHelper = null!;
        private SettingsViewRightPanelController _settingsViewRightPanelController = null!;
        private UIUtils _uiUtils = null!;
        public FlowCoordinator parentFlowCoordinator = null!;

        private bool _inMenu;
        private bool _inPause;
        private Color _bgColor;
        private bool _rememberNsfw;
        private bool _skipNsfw;
        private int _autoNyaWait;
        private bool _separatePositions;
        private Vector3 _menuPosition;
        private Vector3 _menuRotation;
        private Vector3 _pausePosition;
        private Vector3 _pauseRotation;

        [Inject]
        public void Constructor(PluginConfig config, UIUtils uiUtils, MainFlowCoordinator mainFlowCoordinator, MenuTransitionsHelper menuTransitionsHelper, SettingsViewRightPanelController settingsViewRightPanelController)
        {
            _config = config;
            _mainFlowCoordinator = mainFlowCoordinator;
            _menuTransitionsHelper = menuTransitionsHelper;
            _settingsViewRightPanelController = settingsViewRightPanelController;
            _uiUtils = uiUtils;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

            InMenu = _config.InMenu;
            InPause = _config.InPause;
            BgColour = _config.BackgroundColor;
            RememberNsfw = _config.RememberNsfw;
            SkipNsfw = _config.SkipNsfw;
            AutoNyaWait = _config.AutoNyaWait;
            SeparatePositions = _config.SeperatePositions;
            _menuPosition = _config.MenuPosition;
            _menuRotation = _config.MenuRotation;
            _pausePosition = _config.PausePosition;
            _pauseRotation = _config.PauseRotation;
        }

        [UIValue("view-controller-active")]
        private bool ViewControllerActive => isActiveAndEnabled;

        [UIValue("size-delta-view-controller")]
        private int SizeDeltaViewController
        {
            get
            {
                if (isActiveAndEnabled) return -50;
                return 0;
            }
        }

        [UIValue("restart-required")]
        private bool RestartRequired => InMenu != _config.InMenu && isActiveAndEnabled;

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
        private Color BgColour
        {
            get => _bgColor;
            set
            {
                _bgColor = value;
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

        [UIComponent("bg-colour-setting")]
        private readonly Transform _bgColourSettingTransform = null!;

        [UIComponent("bg-colour-default")]
        private readonly Button _bgColourDefaultButton = null!;

        [UIComponent("reset-nya-position")]
        private readonly Button _resetNyaPositionButton = null!;

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
            if (_config.InMenu)
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

        [UIAction("ok-clicked")]
        private void OkClicked()
        {
            var restartRequired = RestartRequired;
            _config.InMenu = InMenu;
            _config.InPause = InPause;
            _config.BackgroundColor = BgColour;
            _config.RememberNsfw = RememberNsfw;
            _config.SkipNsfw = SkipNsfw;
            _config.AutoNyaWait = AutoNyaWait;
            _config.SeperatePositions = SeparatePositions;
            _config.MenuPosition = _menuPosition;
            _config.MenuRotation = _menuRotation;
            _config.PausePosition = _pausePosition;
            _config.PauseRotation = _pauseRotation;

            if (restartRequired) _menuTransitionsHelper.RestartGame();
            else parentFlowCoordinator.DismissFlowCoordinator(_mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf(), animationDirection: AnimationDirection.Vertical);
        }

        [UIAction("cancel-clicked")]
        private void CancelClicked()
        {
            _uiUtils.NyaBgMaterial.color = _config.BackgroundColor;
            if (_config.InMenu)
            {
                var floatingScreen = GameObject.Find("NyaMenuFloatingScreen");
                floatingScreen.transform.position = _menuPosition;
                floatingScreen.transform.rotation = Quaternion.Euler(_menuRotation);
            }
            _settingsViewRightPanelController.gameObject.SetActive(false); // Thank you leaderboard panel for kidnapping my right panel
            parentFlowCoordinator.DismissFlowCoordinator(_mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf(), animationDirection: AnimationDirection.Vertical);
        }

        [UIAction("#post-parse")]
        private void ModSettingsParse()
        {
            if (isActiveAndEnabled) return;
            InMenu = _config.InMenu;
            InPause = _config.InPause;
            BgColour = _config.BackgroundColor;
            RememberNsfw = _config.RememberNsfw;
            SkipNsfw = _config.SkipNsfw;
            AutoNyaWait = _config.AutoNyaWait;
            SeparatePositions = _config.SeperatePositions;
            _menuPosition = _config.MenuPosition;
            _menuRotation = _config.MenuRotation;
            _pausePosition = _config.PausePosition;
            _pauseRotation = _config.PauseRotation;
        }

        [UIAction("#apply")]
        private void ModSettingsApply()
        {
            _config.InMenu = InMenu;
            _config.InPause = InPause;
            _config.BackgroundColor = BgColour;
            _config.RememberNsfw = RememberNsfw;
            _config.SkipNsfw = SkipNsfw;
            _config.AutoNyaWait = AutoNyaWait;
            _config.SeperatePositions = SeparatePositions;
            _config.MenuPosition = _menuPosition;
            _config.MenuRotation = _menuRotation;
            _config.PausePosition = _pausePosition;
            _config.PauseRotation = _pauseRotation;
        }

        [UIAction("#cancel")]
        private void ModSettingsCancel()
        {
            _uiUtils.NyaBgMaterial.color = _config.BackgroundColor;
            if (_config.InMenu)
            {
                var floatingScreen = GameObject.Find("NyaMenuFloatingScreen");
                floatingScreen.transform.position = _menuPosition;
                floatingScreen.transform.rotation = Quaternion.Euler(_menuRotation);
            }
        }

        public void Initialize()
        {
            BSMLSettings.instance.AddSettingsMenu("Nya", "Nya.UI.Views.SettingsViewMainPanel.bsml", this);
        }

        public void Dispose()
        {
            if (BSMLSettings.instance != null)
            {
                BSMLSettings.instance.RemoveSettingsMenu(this);
            }
        }
    }
}