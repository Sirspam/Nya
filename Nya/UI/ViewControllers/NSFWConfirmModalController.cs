using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using UnityEngine;

namespace Nya.UI.ViewControllers
{
    internal class NsfwConfirmModalController
    {
        public delegate void ButtonPressed();

        private ButtonPressed _yesButtonPressed = null!;
        private ButtonPressed _noButtonPressed = null!;

        [UIComponent("modal")]
        private readonly ModalView _modalView = null!;

        [UIComponent("modal")]
        private readonly RectTransform _modalTransform = null!;

        [UIAction("yes-click")]
        private void YesNsfw()
        {
            _yesButtonPressed.Invoke();
            _parserParams.EmitEvent("close-modal");
        }

        [UIAction("no-click")]
        private void NoNsfw()
        {
            _noButtonPressed.Invoke();
            _parserParams.EmitEvent("close-modal");
        }

        [UIParams]
        private readonly BSMLParserParams _parserParams = null!;

        private void Parse(Component parentTransform)
        {
            if (!_modalView)
            {
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.NSFWConfirmModal.bsml"), parentTransform.gameObject, this);
                _modalView.SetField("_animateParentCanvas", false);
                _modalView.name = "NyaNsfwConfirmModal";
            }
        }

        internal void ShowModal(Transform parentTransform, ButtonPressed yesButtonPressedCallback, ButtonPressed noButtonPressedCallback)
        {
            Parse(parentTransform);
            _parserParams.EmitEvent("close-modal");
            _parserParams.EmitEvent("open-modal");
            _yesButtonPressed = yesButtonPressedCallback;
            _noButtonPressed = noButtonPressedCallback;
        }

        internal void HideModal()
        {
            if (_modalTransform != null)
            {
                _modalTransform.GetComponent<ModalView>().Hide(false);
            }
        }
    }
}