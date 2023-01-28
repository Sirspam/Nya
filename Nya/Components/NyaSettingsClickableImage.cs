using System.Reflection;
using BeatSaberMarkupLanguage;
using Nya.Configuration;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Nya.Components
{
	internal sealed class NyaSettingsClickableImage : MonoBehaviour, IPointerClickHandler
	{
		private AudioClip[] _audioClips = new AudioClip[3];
		private AudioSource _basicUIAudioManagerAudioSource = null!;

		private bool _isBogShamb;
		private PluginConfig _pluginConfig = null!;
		private IPlatformUserModel _platformUserModel = null!;

		[Inject]
		public async void Construct(PluginConfig pluginConfig, IPlatformUserModel platformUserModel)
		{
			_pluginConfig = pluginConfig;
			_platformUserModel = platformUserModel;

			_isBogShamb = (await _platformUserModel.GetUserInfo()).platformUserId == "76561198087340992";
		}

		private void Awake()
		{
			if (_audioClips[0] == null)
			{
				GetAudioClips();
			}

			if (_basicUIAudioManagerAudioSource == null)
			{
				_basicUIAudioManagerAudioSource = BeatSaberUI.BasicUIAudioManager.GetComponent<AudioSource>();
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (_pluginConfig.IsAprilFirst || (_isBogShamb && _pluginConfig.EasterEggs))
			{
				_basicUIAudioManagerAudioSource.PlayOneShot(_audioClips[0]);
			}
			else
			{
				_basicUIAudioManagerAudioSource.PlayOneShot(name == "VanillaImage" ? _audioClips[2] : _audioClips[1]);
			}
		}

		private void GetAudioClips()
		{
			var loadedAssetBundle = AssetBundle.LoadFromMemory(Utilities.GetResource(Assembly.GetExecutingAssembly(), "Nya.Resources.nya.bundle"));
			_audioClips = loadedAssetBundle.LoadAllAssets<AudioClip>();
			loadedAssetBundle.Unload(false);
		}
	}
}