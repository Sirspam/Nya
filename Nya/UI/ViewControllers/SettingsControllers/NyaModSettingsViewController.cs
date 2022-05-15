using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using HMUI;
using Nya.UI.FlowCoordinators;
using Zenject;

namespace Nya.UI.ViewControllers.SettingsControllers
{
	internal class NyaModSettingsViewController : IInitializable, IDisposable
	{
		private readonly MainFlowCoordinator _mainFlowCoordinator;
		private readonly NyaSettingsFlowCoordinator _nyaSettingsFlowCoordinator;
		private readonly NyaSettingsMainViewController _nyaSettingsMainViewController;

		public NyaModSettingsViewController(MainFlowCoordinator mainFlowCoordinator, NyaSettingsFlowCoordinator nyaSettingsFlowCoordinator, NyaSettingsMainViewController nyaSettingsMainViewController)
		{
			_mainFlowCoordinator = mainFlowCoordinator;
			_nyaSettingsFlowCoordinator = nyaSettingsFlowCoordinator;
			_nyaSettingsMainViewController = nyaSettingsMainViewController;
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

		[UIAction("settings-image-clicked")]
		private void ResetPositionClicked()
		{
			_nyaSettingsMainViewController.parentFlowCoordinator = _mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
			_nyaSettingsMainViewController.parentFlowCoordinator.PresentFlowCoordinator(_nyaSettingsFlowCoordinator, animationDirection: ViewController.AnimationDirection.Vertical);
		}
	}
}