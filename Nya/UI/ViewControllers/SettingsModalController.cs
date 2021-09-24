using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using Nya.Configuration;
using Nya.Utils;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace Nya.UI.ViewControllers
{
    internal class SettingsModalController : INotifyPropertyChanged
    {
        private readonly NSFWConfirmModalController nsfwConfirmModalController;
        private readonly ButtonUtils _buttonUtils;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool parsed;

        public SettingsModalController(NSFWConfirmModalController nsfwConfirmModalController, ButtonUtils buttonUtils)
        {
            this.nsfwConfirmModalController = nsfwConfirmModalController;
            _buttonUtils = buttonUtils;
            parsed = false;
        }

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

        [UIComponent("nsfwCheckbox")]
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
            if (ImageUtils.nyaImageURL.EndsWith(".gif") || ImageUtils.nyaImageURL.EndsWith(".apng"))
            {
                nyaCopyButton.interactable = false;
            }
            else
            {
                nyaCopyButton.interactable = true;
            }
        }

        #region actions
        [UIAction("nya-download-click")]
        private void downloadNya()
        {
            _buttonUtils.UnderlineClick(nyaDownloadButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
            ImageUtils.DownloadNyaImage();
        }

        [UIAction("nya-copy-click")]
        private void copyNya()
        {
            _buttonUtils.UnderlineClick(nyaCopyButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
            ImageUtils.CopyNyaImage();
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
                PluginConfig.Instance.Changed();
            }
        }
        #endregion
        private void nsfwConfirmYes()
        {
            nsfwCheck = true;
            PluginConfig.Instance.Changed();
        }
        private void nsfwConfirmNo() => nsfwCheck = false;
    }
}