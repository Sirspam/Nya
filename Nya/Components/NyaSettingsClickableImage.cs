using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nya.Components
{
	public class NyaSettingsClickableImage : MonoBehaviour, IPointerClickHandler
	{
		private AudioClip[]? _audioClips;
		private BasicUIAudioManager _basicUIAudioManager = null!;
		
		public void OnPointerClick(PointerEventData eventData)
		{
			if (_audioClips == null)
				GetAudioClips();
			
			// TODO: Replace _BasicUIAudioManager with the one in BSML when that thing releases
			_basicUIAudioManager = Resources.FindObjectsOfTypeAll<BasicUIAudioManager>().First();
			_basicUIAudioManager.GetComponent<AudioSource>().PlayOneShot(name == "VanillaImage" ? _audioClips![1] : _audioClips![0]);
		}

		private void GetAudioClips()
		{
			var loadedAssetBundle = AssetBundle.LoadFromMemory(BeatSaberMarkupLanguage.Utilities.GetResource(Assembly.GetExecutingAssembly(), "Nya.Resources.nya.bundle"));
			_audioClips = loadedAssetBundle.LoadAllAssets<AudioClip>();
			loadedAssetBundle.Unload(false);
		}
	}
}