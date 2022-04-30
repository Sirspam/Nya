using System.Reflection;
using BeatSaberMarkupLanguage;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nya.Components
{
	public class NyaSettingsClickableImage : MonoBehaviour, IPointerClickHandler
	{
		private AudioClip[]? _audioClips;
		private AudioSource _basicUIAudioManagerAudioSource = null!;

		private void Awake()
		{
			if (_audioClips == null)
			{
				GetAudioClips();
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (_audioClips == null)
			{
				GetAudioClips();
			}

			if (_basicUIAudioManagerAudioSource == null)
			{
				_basicUIAudioManagerAudioSource = BeatSaberUI.BasicUIAudioManager.GetComponent<AudioSource>();
			}

			_basicUIAudioManagerAudioSource.PlayOneShot(name == "VanillaImage" ? _audioClips![1] : _audioClips![0]);
		}

		private void GetAudioClips()
		{
			var loadedAssetBundle = AssetBundle.LoadFromMemory(Utilities.GetResource(Assembly.GetExecutingAssembly(), "Nya.Resources.nya.bundle"));
			_audioClips = loadedAssetBundle.LoadAllAssets<AudioClip>();
			loadedAssetBundle.Unload(false);
		}
	}
}