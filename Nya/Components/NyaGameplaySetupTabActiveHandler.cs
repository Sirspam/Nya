using Nya.UI.ViewControllers.NyaViewControllers;
using UnityEngine;
using Zenject;

namespace Nya.Components
{
	internal class NyaGameplaySetupTabActiveHandler : MonoBehaviour
	{
		private NyaViewGameplaySetupController _nyaViewGameplaySetupController = null!;

		[Inject]
		public void Construct(NyaViewGameplaySetupController nyaViewGameplaySetupController)
		{
			_nyaViewGameplaySetupController = nyaViewGameplaySetupController;
		}

		private void OnEnable()
		{
			_nyaViewGameplaySetupController.OnEnable();
		}

		private void OnDisable()
		{
			_nyaViewGameplaySetupController.OnDisable();
		}
	}
}