using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using System.Reflection;
using UnityEngine;

namespace Nya.UI.ViewControllers
{
    public class NsfwConfirmModalController
    {
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

        #endregion components

        [UIAction("yes-click")]
        private void YesNsfw()
        {
            yesButtonPressed?.Invoke();
            parserParams.EmitEvent("close-modal");
        }

        [UIAction("no-click")]
        private void NoNsfw()
        {
            noButtonPressed?.Invoke();
            parserParams.EmitEvent("close-modal");
        }

        [UIParams]
        private readonly BSMLParserParams parserParams;

        private void Parse(Transform parentTransform)
        {
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.NSFWConfirmModal.bsml"), parentTransform.gameObject, this);
            modalView.SetField("_animateParentCanvas", false);
            if (rootTransform != null && modalTransform != null)
            {
                modalTransform.SetParent(rootTransform);
                modalTransform.gameObject.SetActive(false);
            }
        }

        internal void ShowModal(Transform parentTransform, ButtonPressed yesButtonPressedCallback, ButtonPressed noButtonPressedCallback)
        {
            Parse(parentTransform);
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");
            yesButtonPressed = yesButtonPressedCallback;
            noButtonPressed = noButtonPressedCallback;
        }

        internal void HideModal()
        {
            if (modalTransform != null)
            {
                modalTransform.GetComponent<ModalView>().Hide(false);
            }
        }
    }
}