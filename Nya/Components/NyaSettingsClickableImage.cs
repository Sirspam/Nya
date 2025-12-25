using BeatSaberMarkupLanguage;
using Nya.Configuration;
using OculusStudios.Platform.Core;
using System.Reflection;
using System.Threading.Tasks;
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
		private IPlatform _platformUserModel = null!;

		[Inject]
		public void Construct(PluginConfig pluginConfig, IPlatform platformUserModel)
		{
            _pluginConfig = pluginConfig;
			_platformUserModel = platformUserModel;

            _isBogShamb = _platformUserModel.user.userId.ToString() == "76561198087340992";
		}

		private async void Awake()
		{
			if (_audioClips[0] == null)
			{
				await GetAudioClips();
			}

			if (_basicUIAudioManagerAudioSource == null)
			{
				_basicUIAudioManagerAudioSource = BeatSaberUI.BasicUIAudioManager.GetComponentInChildren<AudioSource>();
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

		private async Task<bool> GetAudioClips()
		{
			var loadedAssetBundle = AssetBundle.LoadFromMemory(await Utilities.GetResourceAsync(Assembly.GetExecutingAssembly(), "Nya.Resources.nya.bundle"));
			_audioClips = loadedAssetBundle.LoadAllAssets<AudioClip>();
			loadedAssetBundle.Unload(false);
			return _audioClips.Length >= 3;
        }
	}
}