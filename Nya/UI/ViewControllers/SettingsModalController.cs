using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using Nya.Configuration;
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
        private GameplaySetupViewController gameplaySetupViewController;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool parsed;
        private string folderPath = Environment.CurrentDirectory + "/UserData/Nya";

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
        #endregion

        #region values
        [UIValue("nya-nsfw-check")]
        private bool nsfwCheck
        {
            get => PluginConfig.Instance.NSFW;
            set
            {
                PluginConfig.Instance.NSFW = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PluginConfig.Instance.NSFW)));
            }
        }
        #endregion

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public SettingsModalController(GameplaySetupViewController gameplaySetupViewController)
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
            NyaModifierController.downloadNya();
        }

        [UIAction("nya-copy-click")]
        private void copyNya()
        {
            NyaModifierController.copyNya();
        }

        [UIAction("nya-nsfw")]
        private void nsfwToggle(bool value)
        {
            nsfwCheck = value;
        }
        #endregion
    }
}