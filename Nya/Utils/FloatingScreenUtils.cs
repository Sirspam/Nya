using System;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using Nya.Configuration;
using SiraUtil.Logging;
using Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nya.Utils
{
	internal class FloatingScreenUtils
	{
		public static Vector3 DefaultPosition = new Vector3(0f, 3.75f, 4f);
		public static Quaternion DefaultRotation = Quaternion.Euler(332f, 0f, 0f);
		
		public Vector3 HandleScale;
		private Material? _uiFogBg;
		private bool _usingNyaMaterial;
		private Material? _nyaBgMaterial;
		public FloatingScreen? MenuFloatingScreen;
		public FloatingScreen? GameFloatingScreen;
		
		private readonly PluginConfig _pluginConfig;
		private readonly TimeTweeningManager _timeTweeningManager;

		public FloatingScreenUtils(PluginConfig pluginConfig, TimeTweeningManager timeTweeningManager)
		{
			_pluginConfig = pluginConfig;
			_timeTweeningManager = timeTweeningManager;
		}

		private Material NyaBgMaterial
		{
			get
			{
				return _nyaBgMaterial ??= InstantiateNyaMaterial();
			}
		}
		
		public enum FloatingScreenType
		{
			Menu,
			Game
		}
		
		public void CreateNyaFloatingScreen(object host, FloatingScreenType type)
		{
			switch (type)
			{
				case FloatingScreenType.Game:
					if (_pluginConfig.SeparatePositions)
					{
						GameFloatingScreen = CreateNyaFloatingScreen(host, _pluginConfig.PausePosition, _pluginConfig.PauseRotation);
						GameFloatingScreen.gameObject.name = "NyaGameFloatingScreen";
						break;
					}
					GameFloatingScreen = CreateNyaFloatingScreen(host, _pluginConfig.MenuPosition, _pluginConfig.MenuRotation);
					GameFloatingScreen.gameObject.name = "NyaGameFloatingScreen";
					break;
					
				case FloatingScreenType.Menu:
				default:
					MenuFloatingScreen = CreateNyaFloatingScreen(host, _pluginConfig.MenuPosition, _pluginConfig.MenuRotation);
					MenuFloatingScreen.gameObject.name = "NyaMenuFloatingScreen";
					break;
			}
		}

		private FloatingScreen CreateNyaFloatingScreen(object host, Vector3 position, Vector3 rotation)
		{
			var floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100f, 80f), true, position, Quaternion.Euler(rotation), 220, true);
			
			BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.NyaView.bsml"), floatingScreen.gameObject, host);
			var gameObject = floatingScreen.gameObject;
			gameObject.transform.localScale = new Vector3(0.02f * _pluginConfig.FloatingScreenScale,  0.02f * _pluginConfig.FloatingScreenScale, 0.02f * _pluginConfig.FloatingScreenScale);;
			gameObject.layer = 5;
			
			floatingScreen.handle.transform.localPosition = new Vector3(0f, -floatingScreen.ScreenSize.y / 1.8f, -5f);
			HandleScale = new Vector3(floatingScreen.ScreenSize.x * 0.8f, floatingScreen.ScreenSize.y / 15f, floatingScreen.ScreenSize.y / 15f);
			floatingScreen.handle.transform.localScale = HandleScale;
			floatingScreen.handle.gameObject.layer = 5;
			floatingScreen.HighlightHandle = true;
			if (!_pluginConfig.ShowHandle)
			{
				floatingScreen.handle.gameObject.SetActive(false);
			}

			if (_pluginConfig.UseBackgroundColor)
			{
				floatingScreen.gameObject.transform.GetChild(0).GetComponent<ImageView>().material = NyaBgMaterial;
			}

			if (_pluginConfig.RainbowBackgroundColor)
			{
				ToggleRainbowNyaBg(true);
			}

			return floatingScreen;
		}

		private FloatingScreen? GetActiveFloatingScreen()
		{
			if (MenuFloatingScreen != null && MenuFloatingScreen.gameObject.activeInHierarchy)
			{
				return MenuFloatingScreen;
			}
			if (GameFloatingScreen != null && GameFloatingScreen.gameObject.activeInHierarchy)
			{
				return GameFloatingScreen;
			}

			return null;
		}

		public void ScaleFloatingScreen(float scale)
		{
			if (MenuFloatingScreen != null)
			{
				MenuFloatingScreen.gameObject.transform.localScale = new Vector3(0.02f * scale,  0.02f * scale, 0.02f * scale);
			}
			
			if (GameFloatingScreen != null)
			{
				GameFloatingScreen.gameObject.transform.localScale = new Vector3(0.02f * scale,  0.02f * scale, 0.02f * scale);
			}
		}
		
		public void TweenToDefaultPosition(bool saveValuesToConfig = true)
		{
			var floatingScreen = GetActiveFloatingScreen();

			if (floatingScreen == null)
			{
				return;
			}
			
			_timeTweeningManager.KillAllTweens(floatingScreen);
			var transform = floatingScreen.gameObject.transform;
			var oldPosition = transform.position;
			var oldRotation = transform.rotation;
			var time = (float) Math.Sqrt(Vector3.Distance(oldPosition, DefaultPosition) / 2);
			if (time < 0.5f)
			{
				time = (float) Math.Sqrt(Quaternion.Angle(oldRotation, DefaultRotation) / 100); // Should probably change this
			}
			var positionTween = new FloatTween(0f, 1f, val => transform.position = Vector3.Lerp(oldPosition, DefaultPosition, val), time, EaseType.OutQuart);
			var rotationTween = new FloatTween(0f, 1f, val => transform.rotation = Quaternion.Lerp(oldRotation, DefaultRotation, val), time, EaseType.OutQuart);
			_timeTweeningManager.AddTween(positionTween, floatingScreen);
			_timeTweeningManager.AddTween(rotationTween, floatingScreen);

			if (!saveValuesToConfig)
			{
				return;
			}
			
			if (floatingScreen.gameObject.name == "NyaGameFloatingScreen" && _pluginConfig.SeparatePositions)
			{
				_pluginConfig.PausePosition = DefaultPosition;
				_pluginConfig.PauseRotation = DefaultRotation.eulerAngles;
			}
			else
			{
				_pluginConfig.MenuPosition = DefaultPosition;
				_pluginConfig.MenuRotation = DefaultRotation.eulerAngles;
			}
		}

		public void TweenToHeadset(Camera camera)
		{
			var floatingScreen = GetActiveFloatingScreen();
			
			if (floatingScreen == null)
			{
				return;
			}
			
			_timeTweeningManager.KillAllTweens(floatingScreen);
			var rootTransform = floatingScreen.gameObject.transform;
			var previousRotation = rootTransform.rotation;
			var newRotation = Quaternion.LookRotation(rootTransform.position - camera.transform.position);
			var tween = new FloatTween(0f, 1f, val => floatingScreen.gameObject.transform.rotation = Quaternion.Lerp(previousRotation, newRotation, val), 0.5f, EaseType.OutQuart);
			_timeTweeningManager.AddTween(tween, floatingScreen);
			if (floatingScreen.name == "nyaGameFloatingScreen" && _pluginConfig.SeparatePositions)
			{
				_pluginConfig.PauseRotation = newRotation.eulerAngles;
			}
			else
			{
				_pluginConfig.MenuRotation = newRotation.eulerAngles;
			}
		}

		public void TweenUpright()
		{
			var floatingScreen = GetActiveFloatingScreen();
			
			if (floatingScreen == null)
			{
				return;
			}
			
			_timeTweeningManager.KillAllTweens(floatingScreen);
			var previousRotation = floatingScreen.gameObject.transform.rotation;
			var newRotation = Quaternion.Euler(0f, previousRotation.eulerAngles.y, 0f);
			var tween = new FloatTween(0f, 1f, val => floatingScreen.gameObject.transform.rotation = Quaternion.Lerp(previousRotation, newRotation, val), 0.5f, EaseType.OutQuart);
			_timeTweeningManager.AddTween(tween, floatingScreen);
			if (floatingScreen.name == "nyaGameFloatingScreen" && _pluginConfig.SeparatePositions)
			{
				_pluginConfig.PauseRotation = newRotation.eulerAngles;
			}
			else
			{
				_pluginConfig.MenuRotation = newRotation.eulerAngles;
			}
		}
		
		public void SetNyaMaterialColor(Color color)
        {
            color.a = 0.6f;
            NyaBgMaterial.color = color;
            SetNyaMaterial();
        }

		public void SetStandardMaterial()
		{
			_usingNyaMaterial = false;
			
			if (_uiFogBg == null)
			{
				_uiFogBg = Resources.FindObjectsOfTypeAll<Material>().Last(x => x.name == "UIFogBG");
			}
	                
			if (MenuFloatingScreen != null)
			{
				MenuFloatingScreen.transform.GetChild(0).GetComponent<ImageView>().material = _uiFogBg;
			}
                    
			if (GameFloatingScreen != null)
			{
				GameFloatingScreen.transform.GetChild(0).GetComponent<ImageView>().material = _uiFogBg;
			}
		}

		private void SetNyaMaterial()
		{
			if (_usingNyaMaterial)
			{
				return;
			}
			
			if (MenuFloatingScreen != null)
			{
				MenuFloatingScreen.transform.GetChild(0).GetComponent<ImageView>().material = NyaBgMaterial;
			}
                    
			if (GameFloatingScreen != null)
			{
				GameFloatingScreen.transform.GetChild(0).GetComponent<ImageView>().material = NyaBgMaterial;
			}

			_usingNyaMaterial = true;
		}
        
        public void ToggleRainbowNyaBg(bool active)
        {
	        _timeTweeningManager.KillAllTweens(NyaBgMaterial);
            if (!active)
            {
                if (_pluginConfig.UseBackgroundColor)
                {
                    SetNyaMaterialColor(_pluginConfig.BackgroundColor);
                }
                else
                {
					SetStandardMaterial();
                }
                
                return;
            }

            if (!_pluginConfig.UseBackgroundColor)
            {
	            SetNyaMaterial();
            }
            
            var tween = new FloatTween(0f, 1, val => SetNyaMaterialColor(Color.HSVToRGB(val, 1f, 1f)), 6f, EaseType.Linear)
            {
                loop = true
            };
            _timeTweeningManager.AddTween(tween, NyaBgMaterial);
        }

        private Material InstantiateNyaMaterial()
        {
            var material = Object.Instantiate(new Material(Resources.FindObjectsOfTypeAll<Material>().Last(x => x.name == "UINoGlow")));
            var color = _pluginConfig.BackgroundColor;
            color.a = 0.5f;
            material.color = color;
            return material;
        }
	}
}