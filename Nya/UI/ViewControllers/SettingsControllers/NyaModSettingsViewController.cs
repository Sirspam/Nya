using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using HMUI;
using Nya.Components;
using Nya.Configuration;
using Nya.Utils;
using UnityEngine.UI;
using Zenject;

namespace Nya.UI.ViewControllers.SettingsControllers
{
	internal class NyaModSettingsViewController : IInitializable, IDisposable
	{
		private readonly UIUtils _uiUtils;
		private readonly PluginConfig _pluginConfig;
		private readonly FloatingScreenUtils _floatingScreenUtils;

		public NyaModSettingsViewController(UIUtils uiUtils, PluginConfig pluginConfig, FloatingScreenUtils floatingScreenUtils)
		{
			_uiUtils = uiUtils;
			_pluginConfig = pluginConfig;
			_floatingScreenUtils = floatingScreenUtils;
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
			_pluginConfig.PausePosition = FloatingScreenUtils.DefaultPosition;
			_pluginConfig.PauseRotation = FloatingScreenUtils.DefaultRotation.eulerAngles;
			
			if (_pluginConfig.InMenu && _floatingScreenUtils.MenuFloatingScreen != null)
			{
				// TransitionToDefault saves position to config
				_floatingScreenUtils.TransitionToDefaultPosition();
				return;
			}
			
			_pluginConfig.MenuPosition = FloatingScreenUtils.DefaultPosition;
			_pluginConfig.MenuRotation = FloatingScreenUtils.DefaultRotation.eulerAngles;
		}
	}
}