using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using Nya.Configuration;
using Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Nya.Utils
{
    internal class UIUtils
    {
        public Vector3 HandleScale;
        public Material NyaBgMaterial
        {
            get
            {
                return _nyaBgMaterial ??= InstantiateNyaMaterial();
            }
        }

        private Material? _nyaBgMaterial;
        private Color? _defaultUnderlineColor;

        private readonly PluginConfig _pluginConfig;
        private readonly TimeTweeningManager _uwuTweenyManager; // Thanks PixelBoom

        public UIUtils(PluginConfig pluginConfig, TimeTweeningManager timeTweeningManager)
        {
            _pluginConfig = pluginConfig;
            _uwuTweenyManager = timeTweeningManager;
        }

        public FloatingScreen CreateNyaFloatingScreen(object host, Vector3 position, Quaternion rotation)
        {
            var floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100f, 80f), true, position, rotation, 220, true);
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.NyaView.bsml"), floatingScreen.gameObject, host);
            floatingScreen.gameObject.layer = 5;
            floatingScreen.handle.transform.localPosition = new Vector3(0f, -floatingScreen.ScreenSize.y / 1.8f, -5f);
            HandleScale = new Vector3(floatingScreen.ScreenSize.x * 0.8f, floatingScreen.ScreenSize.y / 15f, floatingScreen.ScreenSize.y / 15f);
            floatingScreen.handle.transform.localScale = HandleScale;
            floatingScreen.handle.gameObject.layer = 5;
            floatingScreen.HighlightHandle = true;
            if (!_pluginConfig.ShowHandle)
                floatingScreen.handle.gameObject.SetActive(false);
            if (_pluginConfig.UseBackgroundColor)
                floatingScreen.gameObject.transform.GetChild(0).GetComponent<ImageView>().material = NyaBgMaterial;
            if (_pluginConfig.RainbowBackgroundColor)
                RainbowNyaBg(true);
            return floatingScreen;
        }

        public async void ButtonUnderlineClick(GameObject gameObject)
        {
            var underline = await Task.Run(() => gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
            _defaultUnderlineColor ??= await Task.Run(() => Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == "BSMLButton")).transform.Find("Underline").gameObject.GetComponent<ImageView>().color);

            _uwuTweenyManager.KillAllTweens(underline);
            var tween = new FloatTween(0f, 1f, val => underline.color = Color.Lerp(new Color(0f, 0.7f, 1f), (Color) _defaultUnderlineColor, val), 1f, EaseType.InSine);
            _uwuTweenyManager.AddTween(tween, underline);
        }
        
        public void SetNyaMaterialColor(Color color)
        {
            if (!_pluginConfig.UseBackgroundColor && _pluginConfig.InMenu)
            {
                GameObject.Find("NyaMenuFloatingScreen").gameObject.transform.GetChild(0).GetComponent<ImageView>().material = NyaBgMaterial;
            }
            
            color.a = 0.55f;
            NyaBgMaterial!.color = color;
        }
        
        public void RainbowNyaBg(bool active)
        {
            _uwuTweenyManager.KillAllTweens(NyaBgMaterial);
            if (!active)
            {
                if (_pluginConfig.UseBackgroundColor)
                {
                    SetNyaMaterialColor(_pluginConfig.BackgroundColor);
                }
                else
                {
                    GameObject.Find("NyaMenuFloatingScreen").gameObject.transform.GetChild(0).GetComponent<ImageView>().material = Resources.FindObjectsOfTypeAll<Material>().Last(x => x.name == "UIFogBG");
                }
                return;
            }

            if (!_pluginConfig.UseBackgroundColor)
            {
                GameObject.Find("NyaMenuFloatingScreen").gameObject.transform.GetChild(0).GetComponent<ImageView>().material = NyaBgMaterial;
            }
            
            var tween = new FloatTween(0f, 1, val => SetNyaMaterialColor(Color.HSVToRGB(val, 1f, 1f)), 6f, EaseType.Linear)
            {
                loop = true
            };
            _uwuTweenyManager.AddTween(tween, NyaBgMaterial);
        }

        private Material InstantiateNyaMaterial()
        {
            var material = Object.Instantiate(new Material(Resources.FindObjectsOfTypeAll<Material>().Last(x => x.name == "UINoGlow"))); // UIFogBG, UINoGlow
            var color = _pluginConfig.BackgroundColor;
            color.a = 0.5f;
            material.color = color;
            return material;
        }
    }
}