using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using Nya.Components;
using Nya.Configuration;
using Nya.UI.ViewControllers.ModalControllers;
using Nya.Utils;
using Zenject;

namespace Nya.UI.ViewControllers.NyaViewControllers
{
	internal class NyaViewGameplaySetupController : NyaViewController, IInitializable, IDisposable
	{
		private readonly DiContainer _diContainer;
		private readonly SettingsModalMenuController _settingsModalMenuController;
		private readonly GameplaySetupViewController _gameplaySetupViewController;
		
		public NyaViewGameplaySetupController(ImageUtils imageUtils, PluginConfig pluginConfig, TickableManager tickableManager, DiContainer diContainer, SettingsModalMenuController settingsModalMenuController, GameplaySetupViewController gameplaySetupViewController)
			: base(imageUtils, pluginConfig, tickableManager)
		{
			_diContainer = diContainer;
			_settingsModalMenuController = settingsModalMenuController;
			_gameplaySetupViewController = gameplaySetupViewController;
		}

		public override void Initialize()
		{
			base.Initialize();
			
			GameplaySetup.instance.AddTab("Nya", "Nya.UI.Views.NyaView.bsml", this);

			_gameplaySetupViewController.didActivateEvent += GameplaySetupViewControllerOnDidActivateEvent;
			_gameplaySetupViewController.didDeactivateEvent += GameplaySetupViewControllerOnDidDeactivateEvent;
		}

		public override void Dispose()
		{
			base.Dispose();
			
			if (GameplaySetup.IsSingletonAvailable)
			{
				GameplaySetup.instance.RemoveTab("Nya");
			}
			
			_gameplaySetupViewController.didActivateEvent -= GameplaySetupViewControllerOnDidActivateEvent;
			_gameplaySetupViewController.didDeactivateEvent -= GameplaySetupViewControllerOnDidDeactivateEvent;
		}

		public void OnEnable()
		{
			if (PluginConfig.PersistantAutoNya && AutoNyaButtonToggle && !AutoNyaActive)
			{
				ToggleAutoNya(true);
			}
		}

		public void OnDisable()
		{
			if (AutoNyaActive)
			{
				ToggleAutoNya(false);
			}
		}

		private void GameplaySetupViewControllerOnDidActivateEvent(bool firstactivation, bool addedtohierarchy, bool screensystemenabling)
		{
			if (firstactivation)
			{
				_diContainer.InstantiateComponent<NyaGameplaySetupTabActiveHandler>(RootTransform.gameObject);
			}
			else
			{
				NyaButton.interactable = false;
				ImageUtils.LoadCurrentNyaImage(NyaImage, () =>
				{
					NyaButton.interactable = true;
					if (PluginConfig.PersistantAutoNya && AutoNyaButtonToggle && !AutoNyaActive)
					{
						ToggleAutoNya(true);
					}
				});
			}
		}

		private void GameplaySetupViewControllerOnDidDeactivateEvent(bool removedfromhierarchy, bool screensystemdisabling)
		{
			if (AutoNyaActive)
			{
				ToggleAutoNya(false);
			}
            
			_settingsModalMenuController.HideModal();
		}

		private void ModalViewOnBlockerClickedEvent()
		{
			if (PluginConfig.PersistantAutoNya && AutoNyaButtonToggle && !AutoNyaActive)
			{
				ToggleAutoNya(true);
			}
		}
		
		[UIAction("settings-button-clicked")]
		protected void SettingsButtonClicked()
		{
			if (AutoNyaActive)
			{
				ToggleAutoNya(false);
			}
            
			_settingsModalMenuController.ShowModal(SettingsButtonTransform);
			_settingsModalMenuController.ModalView.blockerClickedEvent += ModalViewOnBlockerClickedEvent;
		}
	}
}