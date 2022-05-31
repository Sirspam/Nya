using System;
using HMUI;
using Tweening;
using UnityEngine;

namespace Nya.Utils
{
    internal class UIUtils
    {
        private Color? _defaultUnderlineColor;
        private readonly TimeTweeningManager _uwuTweenyManager; // Thanks PixelBoom

        public UIUtils(TimeTweeningManager timeTweeningManager)
        {
            _uwuTweenyManager = timeTweeningManager;
        }
        public void ButtonUnderlineClick(GameObject gameObject)
        {
            var underline = gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>();
            _defaultUnderlineColor ??= underline.color;

            _uwuTweenyManager.KillAllTweens(underline);
            var tween = new FloatTween(0f, 1f, val => underline.color = Color.Lerp(new Color(0f, 0.75f, 1f), (Color) _defaultUnderlineColor, val), 0.6f, EaseType.InQuad);
            _uwuTweenyManager.AddTween(tween, underline);
        }
    }
}