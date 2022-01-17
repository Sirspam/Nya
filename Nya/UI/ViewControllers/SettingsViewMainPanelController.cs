using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using System;
using UnityEngine;
using Zenject;

namespace Nya.UI.ViewControllers
{
    [HotReload(RelativePathToLayout = @"..\Views\SettingsViewMainPanel.bsml")]
    [ViewDefinition("Nya.UI.Views.SettingsViewMainPanel.bsml")]
    public class SettingsViewMainPanelController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        private MainFlowCoordinator mainFlowCoordinator;
        private MenuTransitionsHelper menuTransitionsHelper;
        private SettingsViewRightPanelController settingsViewRightPanelController;
        private UIUtils uiUtils;
        public FlowCoordinator parentFlowCoordinator;

        protected bool _inMenu;
        protected bool _inPause;
        protected Color _bgColor;
        protected bool _rememberNsfw;
        protected bool _skipNsfw;
        protected int _autoNyaWait;
        protected bool _seperatePositions;
        protected Vector3 _menuPosition;
        protected Vector3 _menuRotation;
        protected Vector3 _pausePosition;
        protected Vector3 _pauseRotation;

        [Inject]
        public void Constructor(MainFlowCoordinator mainFlowCoordinator, MenuTransitionsHelper menuTransitionsHelper, SettingsViewRightPanelController settingsViewRightPanelController, UIUtils uiUtils)
        {
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.menuTransitionsHelper = menuTransitionsHelper;
            this.settingsViewRightPanelController = settingsViewRightPanelController;
            this.uiUtils = uiUtils;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

            InMenu = PluginConfig.Instance.InMenu;
            InPause = PluginConfig.Instance.InPause;
            BgColour = PluginConfig.Instance.BackgroundColor;
            RememberNsfw = PluginConfig.Instance.RememberNsfw;
            SkipNsfw = PluginConfig.Instance.SkipNsfw;
            AutoNyaWait = PluginConfig.Instance.AutoNyaWait;
            SeperatePositions = PluginConfig.Instance.SeperatePositions;
            _menuPosition = PluginConfig.Instance.MenuPosition;
            _menuRotation = PluginConfig.Instance.MenuRotation;
            _pausePosition = PluginConfig.Instance.PausePosition;
            _pauseRotation = PluginConfig.Instance.PauseRotation;
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
        private bool RestartRequired => InMenu != PluginConfig.Instance.InMenu && isActiveAndEnabled;

        [UIValue("in-menu")]
        protected bool InMenu
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
        protected bool InPause
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
                uiUtils.NyaBGMaterial.color = value;
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

        [UIValue("seperate-positions")]
        private bool SeperatePositions
        {
            get => _seperatePositions;
            set
            {
                _seperatePositions = value;
                SeperatePositionsButOpposite = !value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("seperate-positions-but-opposite")] // Might be a better way to do this 💀
        private bool SeperatePositionsButOpposite
        {
            get => !SeperatePositions;
            set => NotifyPropertyChanged();
        }

        [UIComponent("bg-colour-setting")]
        private readonly Transform bgColourSettingTransform;

        [UIComponent("bg-colour-default")]
        private readonly UnityEngine.UI.Button bgColourDefaultButton;

        [UIComponent("reset-nya-position")]
        private readonly UnityEngine.UI.Button resetNyaPositionButton;

        [UIComponent("reset-menu-position")]
        private readonly UnityEngine.UI.Button resetMenuPositionButton;

        [UIComponent("reset-pause-position")]
        private readonly UnityEngine.UI.Button resetPausePositionButton;

        [UIAction("bg-colour-default-clicked")]
        private void BgColourDefaultClicked()
        {
            uiUtils.ButtonUnderlineClick(bgColourDefaultButton.gameObject);
            var modalColourPicker = bgColourSettingTransform.GetChild(2).GetComponent<ModalColorPicker>();
            modalColourPicker.CurrentColor = new Color(0.745f, 0.745f, 0.745f);
            modalColourPicker.DonePressed(); // Thank you DonePressed for making everything magically work
        }

        [UIAction("reset-menu-clicked")]
        private void ResetMenuPosition()
        {
            uiUtils.ButtonUnderlineClick(resetMenuPositionButton.gameObject);
            _menuPosition = new Vector3(0f, 3.65f, 4f);
            _menuRotation = new Vector3(335f, 0f, 0f);
            if (PluginConfig.Instance.InMenu)
            {
                var floatingScreen = GameObject.Find("NyaMenuFloatingScreen");
                floatingScreen.transform.position = new Vector3(0f, 3.65f, 4f);
                floatingScreen.transform.rotation = Quaternion.Euler(new Vector3(335f, 0f, 0f));
            }
        }

        [UIAction("reset-pause-clicked")]
        private void ResetPausePosition()
        {
            uiUtils.ButtonUnderlineClick(resetPausePositionButton.gameObject);
            _pausePosition = new Vector3(-2f, 1.5f, 0f);
            _pauseRotation = new Vector3(0f, 270f, 0f);
        }

        [UIAction("ok-clicked")]
        private void OkClicked()
        {
            bool restartRequired = RestartRequired;
            PluginConfig.Instance.InMenu = InMenu;
            PluginConfig.Instance.InPause = InPause;
            PluginConfig.Instance.BackgroundColor = BgColour;
            PluginConfig.Instance.RememberNsfw = RememberNsfw;
            PluginConfig.Instance.SkipNsfw = SkipNsfw;
            PluginConfig.Instance.AutoNyaWait = AutoNyaWait;
            PluginConfig.Instance.SeperatePositions = SeperatePositions;
            PluginConfig.Instance.MenuPosition = _menuPosition;
            PluginConfig.Instance.MenuRotation = _menuRotation;
            PluginConfig.Instance.PausePosition = _pausePosition;
            PluginConfig.Instance.PauseRotation = _pauseRotation;

            if (restartRequired) menuTransitionsHelper.RestartGame();
            else parentFlowCoordinator.DismissFlowCoordinator(mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf(), animationDirection: AnimationDirection.Vertical);
        }

        [UIAction("cancel-clicked")]
        private void CancelClicked()
        {
            uiUtils.NyaBGMaterial.color = PluginConfig.Instance.BackgroundColor;
            if (PluginConfig.Instance.InMenu)
            {
                var floatingScreen = GameObject.Find("NyaMenuFloatingScreen");
                floatingScreen.transform.position = _menuPosition;
                floatingScreen.transform.rotation = Quaternion.Euler(_menuRotation);
            }
            parentFlowCoordinator.DismissFlowCoordinator(mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf(), animationDirection: AnimationDirection.Vertical);
        }

        [UIAction("#post-parse")]
        private void ModSettingsParse()
        {
            if (isActiveAndEnabled) return;
            InMenu = PluginConfig.Instance.InMenu;
            InPause = PluginConfig.Instance.InPause;
            BgColour = PluginConfig.Instance.BackgroundColor;
            RememberNsfw = PluginConfig.Instance.RememberNsfw;
            SkipNsfw = PluginConfig.Instance.SkipNsfw;
            AutoNyaWait = PluginConfig.Instance.AutoNyaWait;
            SeperatePositions = PluginConfig.Instance.SeperatePositions;
            _menuPosition = PluginConfig.Instance.MenuPosition;
            _menuRotation = PluginConfig.Instance.MenuRotation;
            _pausePosition = PluginConfig.Instance.PausePosition;
            _pauseRotation = PluginConfig.Instance.PauseRotation;
        }

        [UIAction("#apply")]
        private void ModSettingsApply()
        {
            PluginConfig.Instance.InMenu = InMenu;
            PluginConfig.Instance.InPause = InPause;
            PluginConfig.Instance.BackgroundColor = BgColour;
            PluginConfig.Instance.RememberNsfw = RememberNsfw;
            PluginConfig.Instance.SkipNsfw = SkipNsfw;
            PluginConfig.Instance.AutoNyaWait = AutoNyaWait;
            PluginConfig.Instance.SeperatePositions = SeperatePositions;
            PluginConfig.Instance.MenuPosition = _menuPosition;
            PluginConfig.Instance.MenuRotation = _menuRotation;
            PluginConfig.Instance.PausePosition = _pausePosition;
            PluginConfig.Instance.PauseRotation = _pauseRotation;
        }

        [UIAction("#cancel")]
        private void ModSettingsCancel()
        {
            uiUtils.NyaBGMaterial.color = PluginConfig.Instance.BackgroundColor;
            if (PluginConfig.Instance.InMenu)
            {
                var floatingScreen = GameObject.Find("NyaMenuFloatingScreen");
                floatingScreen.transform.position = _menuPosition;
                floatingScreen.transform.rotation = Quaternion.Euler(_menuRotation);
            }
        }

        public void Initialize() => BSMLSettings.instance.AddSettingsMenu("Nya", "Nya.UI.Views.SettingsViewMainPanel.bsml", this);

        public void Dispose()
        {
            if (BSMLSettings.instance != null)
            {
                BSMLSettings.instance.RemoveSettingsMenu(this);
            }
        }
    }
}