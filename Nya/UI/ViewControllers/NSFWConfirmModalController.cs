using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using System;
using BeatSaberMarkupLanguage.Parser;
using Nya.Configuration;
using Nya.Utils;
using HMUI;
using IPA.Utilities;
using System.Reflection;
using UnityEngine;
using Zenject;
using System.ComponentModel;


namespace Nya.UI.ViewControllers
{
    public class NSFWConfirmModalController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        private GameplaySetupViewController gameplaySetupViewController;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool parsed;

        public delegate void ButtonPressed();
        private ButtonPressed yesButtonPressed;
        private ButtonPressed noButtonPressed;

        #region components
        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("modal")]
        private ModalView modalView;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;
        #endregion

        [UIAction("yes-click")]
        private void yesNSFW()
        {
            yesButtonPressed?.Invoke();
            parserParams.EmitEvent("close-modal");
        }

        [UIAction("no-click")]
        private void noNSFW()
        {
            noButtonPressed?.Invoke();
            parserParams.EmitEvent("close-modal");
        }

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public NSFWConfirmModalController(GameplaySetupViewController gameplaySetupViewController)
        {
            this.gameplaySetupViewController = gameplaySetupViewController;
            parsed = false;
        }

        public void Initialize()
        {
            gameplaySetupViewController.didDeactivateEvent += GameplaySetupViewController_didActivateEvent;
        }

        public void Dispose()
        {
            gameplaySetupViewController.didDeactivateEvent -= GameplaySetupViewController_didActivateEvent;
        }

        private void GameplaySetupViewController_didActivateEvent(bool firstActivation, bool addedToHierarchy)
        {
            if (parsed && rootTransform != null && modalTransform != null)
            {
                modalTransform.SetParent(rootTransform);
                modalTransform.gameObject.SetActive(false);
            }
        }

        private void Parse(Transform parentTransform)
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.NSFWConfirmModal.bsml"), parentTransform.gameObject, this);
            FieldAccessor<ModalView, bool>.Set(ref modalView, "_animateParentCanvas", true);
        }

        internal void ShowModal(Transform parentTransform, ButtonPressed yesButtonPressedCallback, ButtonPressed noButtonPressedCallback)
        {
            Parse(parentTransform);
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");
            yesButtonPressed = yesButtonPressedCallback;
            noButtonPressed = noButtonPressedCallback;
        }
    }
}
