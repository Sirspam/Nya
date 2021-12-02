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
        private readonly UIUtils _uiUtils;
        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsModalController(NSFWConfirmModalController nsfwConfirmModalController, UIUtils uiUtils)
        {
            this.nsfwConfirmModalController = nsfwConfirmModalController;
            _uiUtils = uiUtils;
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

        private void Parse(Transform parentTransform)
        {
            if (!modalView)
            {
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.SettingsModal.bsml"), parentTransform.gameObject, this);
                FieldAccessor<ModalView, bool>.Set(ref modalView, "_animateParentCanvas", true);
                modalView.name = "NyaSettingsModal";
            }
        }

        internal void ShowModal(Transform parentTransform)
        {   
            Parse(parentTransform);
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");

            var root = parentTransform.root;
            if (root.name == "NyaMenuFloatingScreen" || root.name == "NyaGameFloatingScreen")
            {
                foreach (HoverHint hoverComponent in root.gameObject.GetComponentsInChildren<HoverHint>())
                {
                    hoverComponent.enabled = false;
                }
            }

            if (ImageUtils.nyaImageURL.EndsWith(".gif") || ImageUtils.nyaImageURL.EndsWith(".apng"))
            {
                nyaCopyButton.interactable = false;
            }
            else
            {
                nyaCopyButton.interactable = true;
            }
        }

        internal void HideModal()
        {
            if (modalTransform != null)
            {
                modalTransform.GetComponent<ModalView>().Hide(false);
                nsfwConfirmModalController.HideModal();
            }
        }

        #region actions
        [UIAction("nya-download-click")]
        private void downloadNya()
        {
            _uiUtils.ButtonUnderlineClick(nyaDownloadButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
            ImageUtils.DownloadNyaImage();
        }

        [UIAction("nya-copy-click")]
        private void copyNya()
        {
            _uiUtils.ButtonUnderlineClick(nyaCopyButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
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