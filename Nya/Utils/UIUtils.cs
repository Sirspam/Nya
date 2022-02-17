using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using Nya.Configuration;
using Tweening;
using UnityEngine;

namespace Nya.Utils
{
    internal class UIUtils
    {
        private readonly PluginConfig _config;
        private readonly TimeTweeningManager _uwuTweenyManager; // Thanks PixelBoom

        public Material NyaBgMaterial { get; }

        public UIUtils(PluginConfig config, TimeTweeningManager timeTweeningManager)
        {
            _config = config;
            _uwuTweenyManager = timeTweeningManager;

            NyaBgMaterial = new Material(Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "UIFogBG")) // UIFogBG, UINoGlow
            {
                name = "NyaBG",
                color = _config.BackgroundColor
            };
        }

        public FloatingScreen CreateNyaFloatingScreen(object host, Vector3 position, Quaternion rotation)
        {
            var floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100f, 80f), true, position, rotation, 220, true);
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.NyaView.bsml"), floatingScreen.gameObject, host);
            floatingScreen.gameObject.layer = 5;
            floatingScreen.gameObject.transform.GetChild(0).GetComponent<ImageView>().material = NyaBgMaterial;
            floatingScreen.handle.transform.localPosition = new Vector3(0f, -floatingScreen.ScreenSize.y / 1.8f, -5f);
            floatingScreen.handle.transform.localScale = new Vector3(floatingScreen.ScreenSize.x * 0.8f, floatingScreen.ScreenSize.y / 15f, floatingScreen.ScreenSize.y / 15f);
            floatingScreen.handle.gameObject.layer = 5;
            floatingScreen.HighlightHandle = true;
            if (!_config.ShowHandle)
                floatingScreen.handle.gameObject.SetActive(false);
            if (_config.RainbowBackgroundColor)
                RainbowNyaBg(true);
            return floatingScreen;
        }

        public async void ButtonUnderlineClick(GameObject gameObject)
        {
            var underline = await Task.Run(() => gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
            var originalColor = underline.color;
            _uwuTweenyManager.KillAllTweens(underline);
            var tween = new FloatTween(0f, 1f, val => underline.color = Color.Lerp(new Color(0f, 0.7f, 1f), originalColor, val), 1f, EaseType.InSine);
            _uwuTweenyManager.AddTween(tween, underline);
        }

        public void RainbowNyaBg(bool active)
        {
            _uwuTweenyManager.KillAllTweens(NyaBgMaterial);
            if (!active)
            {
                NyaBgMaterial.color = _config.BackgroundColor;
                return;
            }
            var tween = new FloatTween(0f, 1, val => NyaBgMaterial.color = Color.HSVToRGB(val, 1f, 1f), 5f, EaseType.Linear)
            {
                loop = true
            };
            _uwuTweenyManager.AddTween(tween, NyaBgMaterial);
        }
    }
}