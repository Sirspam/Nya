using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using System;
using UnityEngine;
using Zenject;

namespace Nya.UI.ViewControllers
{
    internal class NyaViewGameController : NyaViewController, IInitializable, IDisposable
    {
        private readonly UIUtils _uiUtils;
        private readonly IGamePause _gamePause;
        private FloatingScreen floatingScreen;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        private HoverHintController hoverHintController;

        public NyaViewGameController(IGamePause gamePause, UIUtils uiUtils, SettingsModalController settingsModalController) : base(settingsModalController)
        {
            _gamePause = gamePause;
            _uiUtils = uiUtils;
        }
        public void Initialize()
        {
            _gamePause.didPauseEvent += GamePause_didPauseEvent;
            _gamePause.willResumeEvent += GamePause_didResumeEvent;
        }

        public void Dispose()
        {
            floatingScreen.gameObject.SetActive(false);
            _gamePause.didPauseEvent -= GamePause_didPauseEvent;
            _gamePause.willResumeEvent -= GamePause_didResumeEvent;
        }

        private void GamePause_didPauseEvent()
        {
            if (floatingScreen == null)
            {
                floatingScreen = _uiUtils.CreateNyaFloatingScreen(this, PluginConfig.Instance.pausePosition, Quaternion.Euler(PluginConfig.Instance.pauseRotation));
                floatingScreen.gameObject.name = "NyaGameFloatingScreen";
            }
            floatingScreen.gameObject.SetActive(true);
        }

        private void GamePause_didResumeEvent()
        {
            if (autoNyaToggle)
            {
                autoNyaToggle = false;
                nyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f);
                nyaButton.interactable = true;
            }
            settingsModalController.HideModal();
            floatingScreen.gameObject.SetActive(false);
        }
    }
}
