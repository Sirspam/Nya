using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using Nya.Configuration;
using Nya.Utils;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Nya.UI.ViewControllers
{
    [HotReload(RelativePathToLayout = @"..\Views\SettingsView.bsml")]
    [ViewDefinition("Nya.UI.Views.SettingsView.bsml")]
    public class SettingsViewController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        private MainFlowCoordinator mainFlowCoordinator;
        private MenuTransitionsHelper menuTransitionsHelper;
        private UIUtils uiUtils;

        [Inject]
        public void Constructor(MainFlowCoordinator mainFlowCoordinator, MenuTransitionsHelper menuTransitionsHelper, UIUtils uiUtils)
        {
            this.mainFlowCoordinator = mainFlowCoordinator;
            this.menuTransitionsHelper = menuTransitionsHelper;
            this.uiUtils = uiUtils;
        }

        //protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        //{
        //    base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        //}

        [UIValue("easter-eggs")]
        private bool EasterEggs
        {
            get => PluginConfig.Instance.EasterEggs;
            set => PluginConfig.Instance.EasterEggs = value;
        }

        [UIValue("bg-colour")]
        private Color bgColour
        {
            get => PluginConfig.Instance.BackgroundColor;
            set
            {
                Plugin.Log.Debug("Yes this is running, no you're not insane");
                uiUtils.NyaBGMaterial.color = value;
                PluginConfig.Instance.BackgroundColor = value;

                uiUtils.NyaBGMaterial = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "UIFogBG");
            }
        }

        [UIValue("remember-NSFW")]
        private bool RememberNsfw
        {
            get => PluginConfig.Instance.RememberNsfw;
            set => PluginConfig.Instance.RememberNsfw = value;
        }

        [UIValue("skip-NSFW")]
        private bool SkipNsfw
        {
            get => PluginConfig.Instance.SkipNsfw;
            set => PluginConfig.Instance.SkipNsfw = value;
        }

        [UIValue("auto-wait-value")]
        private int AutoNyaWait
        {
            get => PluginConfig.Instance.AutoNyaWait;
            set => PluginConfig.Instance.AutoNyaWait = value;
        }

        [UIValue("seperate-positions")]
        private bool seperatePositions
        {
            get => PluginConfig.Instance.SeperatePositions;
            set
            { 
                PluginConfig.Instance.SeperatePositions = value;
                seperatePositionsButOpposite = !value;
                NotifyPropertyChanged("seperatePositions");
            }
        }

        [UIValue("seperate-positions-but-opposite")] // Might be a better way to do this 💀
        private bool seperatePositionsButOpposite
        {
            get => !seperatePositions;
            set => NotifyPropertyChanged("seperatePositionsButOpposite");
        }

        [UIValue("view-controller-active")]
        private bool ViewControllerActive { get => isActiveAndEnabled; }

        [UIValue("size-delta-view-controller")]

        private int SizeDeltaViewController
        {
            get
            {
                if (isActiveAndEnabled) return -50;
                return 0;
            }
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
        private void bgColourDefaultClicked()
        {
            uiUtils.ButtonUnderlineClick(bgColourDefaultButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
            var modalColourPicker = bgColourSettingTransform.GetChild(2).GetComponent<ModalColorPicker>();
            modalColourPicker.CurrentColor = new Color(0.745f, 0.745f, 0.745f);
            modalColourPicker.DonePressed(); // Thank you DonePressed for making everything magically work
        }
        
        [UIAction("reset-menu-clicked")]
        private void ResetMenuPosition()
        {
            uiUtils.ButtonUnderlineClick(resetMenuPositionButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
            PluginConfig.Instance.MenuPosition = new Vector3(0f, 3.65f, 4f);
            PluginConfig.Instance.MenuRotation = new Vector3(335f, 0f, 0f);
        }

        [UIAction("reset-pause-clicked")]
        private void ResetPausePosition()
        {
            uiUtils.ButtonUnderlineClick(resetPausePositionButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
            PluginConfig.Instance.PausePosition = new Vector3(-2f, 1.5f, 0f);
            PluginConfig.Instance.PauseRotation = new Vector3(0f, 270f, 0f);
        }

        [UIAction("ok-clicked")]
        private void OkClicked()
        {
            menuTransitionsHelper.RestartGame();
        }

        [UIAction("cancel-clicked")]
        private void CancelClicked()
        {
            ReflectionUtil.InvokeMethod<object, FlowCoordinator>(mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf(), "DismissViewController", this, AnimationDirection.Vertical, null, false);
        }

        public void Initialize() => BSMLSettings.instance.AddSettingsMenu("Nya", "Nya.UI.Views.SettingsView.bsml", this);
        public void Dispose() => BSMLSettings.instance?.RemoveSettingsMenu(this);
    }
}