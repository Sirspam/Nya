using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using UnityEngine;

namespace Nya.UI.ViewControllers
{
    public class NsfwConfirmModalController
    {
        public delegate void ButtonPressed();

        private ButtonPressed _yesButtonPressed = null!;
        private ButtonPressed _noButtonPressed = null!;

        #region components
        
        [UIComponent("modal")]
        private ModalView modalView = null!;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform = null!;

        #endregion components

        [UIAction("yes-click")]
        private void YesNsfw()
        {
            _yesButtonPressed.Invoke();
            parserParams.EmitEvent("close-modal");
        }

        [UIAction("no-click")]
        private void NoNsfw()
        {
            _noButtonPressed.Invoke();
            parserParams.EmitEvent("close-modal");
        }

        [UIParams]
        private readonly BSMLParserParams parserParams = null!;

        private void Parse(Component parentTransform)
        {
            if (!modalView)
            {
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.NSFWConfirmModal.bsml"), parentTransform.gameObject, this);
                modalView.SetField("_animateParentCanvas", false);
                modalView.name = "NyaNSFWConfirmaModal";
            }
        }

        internal void ShowModal(Transform parentTransform, ButtonPressed yesButtonPressedCallback, ButtonPressed noButtonPressedCallback)
        {
            Parse(parentTransform);
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");
            _yesButtonPressed = yesButtonPressedCallback;
            _noButtonPressed = noButtonPressedCallback;
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