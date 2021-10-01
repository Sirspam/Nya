using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using Nya.Configuration;
using System.Reflection;
using Tweening;
using UnityEngine;

namespace Nya.Utils
{
    internal class UIUtils
    {
        private readonly TimeTweeningManager timeTweeningManager;
        private FloatingScreen _floatingScreen;

        public UIUtils(TimeTweeningManager timeTweeningManager)
        {
            this.timeTweeningManager = timeTweeningManager;
        }

        public FloatingScreen CreateNyaFloatingScreen(object host, Vector3 position, Quaternion rotation)
        {
            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100f, 80f), PluginConfig.Instance.showHandle, position, rotation, curvatureRadius: 220, true);
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.NyaView.bsml"), _floatingScreen.gameObject, host);
            if (_floatingScreen.handle != null)
            {
                _floatingScreen.handle.transform.localPosition = new Vector3(0f, -_floatingScreen.ScreenSize.y / 1.8f, -5f);
                _floatingScreen.handle.transform.localScale = new Vector3(_floatingScreen.ScreenSize.x * 0.8f, _floatingScreen.ScreenSize.y / 15f, _floatingScreen.ScreenSize.y / 15f);
                _floatingScreen.handle.GetComponent<MeshRenderer>().material.color = Color.blue; // Changes pratically nothing but I think it looks a neater :)
                _floatingScreen.handle.gameObject.layer = 5;
                _floatingScreen.HighlightHandle = true;
                _floatingScreen.HandleReleased += FloatingScreen_HandleReleased;
            }
            _floatingScreen.gameObject.layer = 5;
            return _floatingScreen;
        }

        private void FloatingScreen_HandleReleased(object sender, FloatingScreenHandleEventArgs args)
        {
            if (sender.ToString() == "NyaMenuFloatingScreen (BeatSaberMarkupLanguage.FloatingScreen.FloatingScreen)")
            {
                PluginConfig.Instance.menuPosition = _floatingScreen.transform.position;
                PluginConfig.Instance.menuRotation = _floatingScreen.transform.eulerAngles;
            }
            else if (sender.ToString() == "NyaGameFloatingScreen (BeatSaberMarkupLanguage.FloatingScreen.FloatingScreen)")
            {
                PluginConfig.Instance.pausePosition = _floatingScreen.transform.position;
                PluginConfig.Instance.pauseRotation = _floatingScreen.transform.eulerAngles;
            }
        }


        public void ButtonUnderlineClick(ImageView underline)
        {
            Color oldColor = underline.color;
            timeTweeningManager.KillAllTweens(underline);
            var tween = new FloatTween(0f, 1f, val => underline.color = Color.Lerp(new Color(0f, 0.502f, 1f, 1f), oldColor, val), 1f, EaseType.InOutSine);
            timeTweeningManager.AddTween(tween, underline);
        }
    }
}
