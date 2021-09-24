using HMUI;
using Tweening;
using UnityEngine;
using Zenject;

namespace Nya.Utils
{
    internal class ButtonUtils
    {
        private readonly TweeningManager tweeningManager;
        public ButtonUtils(TweeningManager tweeningManager)
        {
            this.tweeningManager = tweeningManager;
        }

        public void UnderlineClick(ImageView underline)
        {
            Color oldColor = underline.color;
            tweeningManager.KillAllTweens(underline);
            var tween = new FloatTween(0f, 1f, val => underline.color = Color.Lerp(new Color(0f, 0.502f, 1f, 1f), oldColor, val), 1f, EaseType.InOutSine);
            tweeningManager.AddTween(tween, underline);
        }
    }
}
