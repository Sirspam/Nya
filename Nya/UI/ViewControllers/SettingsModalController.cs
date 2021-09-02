using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using Nya.Configuration;
using Nya.Utils;
using HMUI;
using IPA.Utilities;
using System;
using System.Reflection;
using UnityEngine;
using Zenject;
using System.ComponentModel;

namespace Nya.UI.ViewControllers
{
    public class SettingsModalController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        private NSFWConfirmModalController nsfwConfirmModalController;
        private GameplaySetupViewController gameplaySetupViewController;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool parsed;

        #region components
        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("modal")]
        private ModalView modalView;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        [UIComponent("nyaDownloadButton")]
        private readonly UnityEngine.UI.Button nyaDownloadButton;

        [UIComponent("nyaCopyButton")]
        private readonly UnityEngine.UI.Button nyaCopyButton;

        [UIComponent("nsfwCheckbox")] // nsfwCheckbox
        private readonly RectTransform nsfwCheckbox;
        #endregion

        #region values
        [UIValue("nya-nsfw-check")]
        private bool nsfwCheck
        {
            get => PluginConfig.Instance.NSFW;
            set
            {
                PluginConfig.Instance.NSFW = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(nsfwCheck)));
            }
        }
        #endregion

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public SettingsModalController(GameplaySetupViewController gameplaySetupViewController, NSFWConfirmModalController nsfwConfirmModalController)
        {
            this.gameplaySetupViewController = gameplaySetupViewController;
            this.nsfwConfirmModalController = nsfwConfirmModalController;
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
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.SettingsModal.bsml"), parentTransform.gameObject, this);
            FieldAccessor<ModalView, bool>.Set(ref modalView, "_animateParentCanvas", true);
        }

        internal void ShowModal(Transform parentTransform)
        {
            Parse(parentTransform);
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");
        }

        #region actions
        [UIAction("nya-download-click")]
        private void downloadNya()
        {
            ImageUtils.downloadNyaImage();
        }

        [UIAction("nya-copy-click")]
        private void copyNya()
        {
            ImageUtils.copyNyaImage();
        }

        [UIAction("nya-nsfw")]
        private void nsfwToggle(bool value)
        {
            if (value && !PluginConfig.Instance.skipNSFW)
            {
                nsfwConfirmModalController.ShowModal(nsfwCheckbox, nsfwConfirmYes, nsfwConfirmNo);
            }
            else
            {
                nsfwCheck = value;
            }
        }
        #endregion
        private void nsfwConfirmYes() => nsfwCheck = true;
        private void nsfwConfirmNo() => nsfwCheck = false;
    }
}