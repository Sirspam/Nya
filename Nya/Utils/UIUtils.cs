using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using Nya.Configuration;
using System.Linq;
using System.Reflection;
using Tweening;
using UnityEngine;

namespace Nya.Utils
{
    public class UIUtils
    {
        private readonly TimeTweeningManager uwuTweenyManager; // Thanks PixelBoom
        public Material NyaBGMaterial 
        { 
            get 
            {
                var material = new Material(Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "UINoGlow")); // UIFogBG
                material.name = "NyaBG";
                material.color = PluginConfig.Instance.BackgroundColor;
                return material;
            }
            set { }
        }
       
        //new Material(Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "UIFogBG"));

        public UIUtils(TimeTweeningManager timeTweeningManager)
        {
            this.uwuTweenyManager = timeTweeningManager;
        }

        public FloatingScreen CreateNyaFloatingScreen(object host, Vector3 position, Quaternion rotation)
        {
            var floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100f, 80f), true, position, rotation, curvatureRadius: 220, true);
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.NyaView.bsml"), floatingScreen.gameObject, host);
            floatingScreen.gameObject.layer = 5;
            floatingScreen.gameObject.transform.GetChild(0).GetComponent<ImageView>().material = NyaBGMaterial;
            // floatingScreen.gameObject.transform.GetChild(0).GetComponent<ImageView>().material.color = PluginConfig.Instance.BackgroundColor;

            floatingScreen.handle.transform.localPosition = new Vector3(0f, -floatingScreen.ScreenSize.y / 1.8f, -5f);
            floatingScreen.handle.transform.localScale = new Vector3(floatingScreen.ScreenSize.x * 0.8f, floatingScreen.ScreenSize.y / 15f, floatingScreen.ScreenSize.y / 15f);
            floatingScreen.handle.gameObject.layer = 5;
            floatingScreen.HighlightHandle = true;
            if (!PluginConfig.Instance.ShowHandle)
            {
                floatingScreen.handle.gameObject.SetActive(false);
            }
            return floatingScreen;
        }

        public void ButtonUnderlineClick(ImageView underline)
        {
            uwuTweenyManager.KillAllTweens(underline);
            var tween = new FloatTween(0f, 1f, val => underline.color = Color.Lerp(new Color(0f, 0.502f, 1f, 1f), new Color(1f, 1f, 1f, 0.502f), val), 1f, EaseType.InOutSine);
            uwuTweenyManager.AddTween(tween, underline);
        }

        public void CanvasFadeOut(CanvasGroup canvasGroup, float time)
        {
            uwuTweenyManager.KillAllTweens(canvasGroup);
            var tween = new FloatTween(0f, 1f, val => canvasGroup.alpha = Mathf.Lerp(1f, 0f, val), time, EaseType.InOutSine);
            uwuTweenyManager.AddTween(tween, canvasGroup);
        }
    }
}