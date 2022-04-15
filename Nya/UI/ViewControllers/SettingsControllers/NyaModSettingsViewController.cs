using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using HMUI;
using Nya.Components;
using Nya.Configuration;
using Nya.Utils;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Nya.UI.ViewControllers.SettingsControllers
{
	internal class NyaModSettingsViewController : IInitializable, IDisposable
	{
		private readonly UIUtils _uiUtils;
		private readonly PluginConfig _pluginConfig;

		public NyaModSettingsViewController(UIUtils uiUtils, PluginConfig pluginConfig)
		{
			_uiUtils = uiUtils;
			_pluginConfig = pluginConfig;
		}

		public void Initialize()
		{
			BSMLSettings.instance.AddSettingsMenu("Nya", "Nya.UI.Views.NyaModSettingsView.bsml", this);
		}

		public void Dispose()
		{
			if (BSMLSettings.instance != null)
			{
				BSMLSettings.instance.RemoveSettingsMenu(this);
			}
		}
		
		[UIComponent("reset-button")]
		private readonly Button _resetButton = null!;
		
		[UIComponent("chocola-image")]
		private readonly ImageView _chocolaImage = null!;
        
		[UIComponent("vanilla-image")]
		private readonly ImageView _vanillaImage = null!;

		[UIAction("#post-parse")]
		private void PostParse()
		{
			// This is absolutely crucial for this view which will be rarely used 
			_chocolaImage.name = "ChocolaImage";
			_vanillaImage.name = "VanillaImage";
			_chocolaImage.gameObject.AddComponent<NyaSettingsClickableImage>();
			_vanillaImage.gameObject.AddComponent<NyaSettingsClickableImage>();
		}

		[UIAction("reset-position-clicked")]
		private void ResetPositionClicked()
		{
			_uiUtils.ButtonUnderlineClick(_resetButton.gameObject);
			_pluginConfig.PausePosition = new Vector3(-2f, 1.5f, 0f);
			_pluginConfig.PauseRotation = new Vector3(0f, 270f, 0f);
			_pluginConfig.MenuPosition = new Vector3(0f, 3.65f, 4f);
			_pluginConfig.MenuRotation = new Vector3(335f, 0f, 0f);
			if (_pluginConfig.InMenu)
			{
				var floatingScreen = GameObject.Find("NyaMenuFloatingScreen");
				floatingScreen.transform.position = new Vector3(0f, 3.65f, 4f);
				floatingScreen.transform.rotation = Quaternion.Euler(new Vector3(335f, 0f, 0f));
			}
		}
	}
}