using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using Nya.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tweening;
using UnityEngine;

namespace Nya.Utils
{
    public class UIUtils
    {
        private readonly TimeTweeningManager uwuTweenyManager; // Thanks PixelBoom

        public Material NyaBGMaterial = new Material(Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "UIFogBG")) // UIFogBG, UINoGlow
        {
            name = "NyaBG",
            color = PluginConfig.Instance.BackgroundColor,
        };

        public UIUtils(TimeTweeningManager timeTweeningManager)
        {
            this.uwuTweenyManager = timeTweeningManager;
        }

        public FloatingScreen CreateNyaFloatingScreen(object host, Vector3 position, Quaternion rotation)
        {
            FloatingScreen floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100f, 80f), true, position, rotation, curvatureRadius: 220, true);
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.NyaView.bsml"), floatingScreen.gameObject, host);
            floatingScreen.gameObject.layer = 5;
            floatingScreen.gameObject.transform.GetChild(0).GetComponent<ImageView>().material = NyaBGMaterial;
            floatingScreen.handle.transform.localPosition = new Vector3(0f, -floatingScreen.ScreenSize.y / 1.8f, -5f);
            floatingScreen.handle.transform.localScale = new Vector3(floatingScreen.ScreenSize.x * 0.8f, floatingScreen.ScreenSize.y / 15f, floatingScreen.ScreenSize.y / 15f);
            floatingScreen.handle.gameObject.layer = 5;
            floatingScreen.HighlightHandle = true;
            if (!PluginConfig.Instance.ShowHandle) floatingScreen.handle.gameObject.SetActive(false);
            if (PluginConfig.Instance.RainbowBackgroundColor) RainbowNyaBG(true);
            return floatingScreen;
        }

        public async void ButtonUnderlineClick(GameObject gameObject)
        {
            ImageView underline = await Task.Run(() => gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
            Color originalColor = underline.color;
            uwuTweenyManager.KillAllTweens(underline);
            FloatTween tween = new FloatTween(0f, 1f, val => underline.color = Color.Lerp(new Color(0f, 0.7f, 1f), originalColor, val), 1f, EaseType.InSine);
            uwuTweenyManager.AddTween(tween, underline);
        }

        public void RainbowNyaBG(bool active)
        {
            uwuTweenyManager.KillAllTweens(NyaBGMaterial);
            if (!active)
            {
                NyaBGMaterial.color = PluginConfig.Instance.BackgroundColor;
                return;
            }
            FloatTween tween = new FloatTween(0f, 1, val => NyaBGMaterial.color = Color.HSVToRGB(val, 1f, 1f), 5f, EaseType.Linear)
            {
                loop = true,
            };
            uwuTweenyManager.AddTween(tween, NyaBGMaterial);
        }
    }
}