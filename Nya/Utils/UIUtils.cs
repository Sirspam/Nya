using System.Linq;
using System.Threading.Tasks;
using HMUI;
using Nya.Configuration;
using Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Nya.Utils
{
    internal class UIUtils
    {
        private Color? _defaultUnderlineColor;

        private readonly PluginConfig _pluginConfig;
        private readonly TimeTweeningManager _uwuTweenyManager; // Thanks PixelBoom

        public UIUtils(PluginConfig pluginConfig, TimeTweeningManager timeTweeningManager)
        {
            _pluginConfig = pluginConfig;
            _uwuTweenyManager = timeTweeningManager;
        }

        public async void ButtonUnderlineClick(GameObject gameObject)
        {
            var underline = await Task.Run(() => gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>());
            _defaultUnderlineColor ??= await Task.Run(() => Resources.FindObjectsOfTypeAll<Button>().Last(x => x.name == "BSMLButton").transform.Find("Underline").gameObject.GetComponent<ImageView>().color);

            _uwuTweenyManager.KillAllTweens(underline);
            var tween = new FloatTween(0f, 1f, val => underline.color = Color.Lerp(new Color(0f, 0.7f, 1f), (Color) _defaultUnderlineColor, val), 1f, EaseType.InSine);
            _uwuTweenyManager.AddTween(tween, underline);
        }
    }
}