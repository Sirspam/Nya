using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using System;
using System.Threading.Tasks;
using Tweening;
using UnityEngine;
using Zenject;

namespace Nya.UI.ViewControllers
{
    public class NyaViewGameController : NyaViewController, IInitializable, IDisposable
    {
        private readonly UIUtils uiUtils;
        private readonly SettingsModalGameController settingsModalGameController;
        private readonly IGamePause gamePause;
        private readonly IDifficultyBeatmap beatmap;
        private readonly TimeTweeningManager timeTweeningManager;
        private readonly IAudioTimeSource audioTimeSource;
        private FloatingScreen floatingScreen;

        public NyaViewGameController(UIUtils uiUtils, SettingsModalGameController settingsModalGameController, IDifficultyBeatmap beatmap, IGamePause gamePause, TimeTweeningManager timeTweeningManager, IAudioTimeSource audioTimeSource)
        {
            this.uiUtils = uiUtils;
            this.settingsModalGameController = settingsModalGameController;
            this.gamePause = gamePause;
            this.beatmap = beatmap;
            this.timeTweeningManager = timeTweeningManager;
            this.audioTimeSource = audioTimeSource;
        }

        public void Initialize()
        {
            floatingScreen = uiUtils.CreateNyaFloatingScreen(this, PluginConfig.Instance.PausePosition, Quaternion.Euler(PluginConfig.Instance.PauseRotation));
            floatingScreen.gameObject.name = "NyaGameFloatingScreen";

            if (PluginConfig.Instance.EasterEggs && beatmap.level.levelID == "custom_level_69E494F4A295197BF03720029086FABE6856FBCE") // e970 my beloved
            {
                // while (!audioTimeSource.isReady)
                // {
                    // Plugin.Log.Debug("weeeee!!"); // I'll improve this later I swear
                    // Task.Delay(25);
                // }
                floatingScreen.handle.SetActive(false);
                nyaButton.gameObject.SetActive(false);
                nyaAutoButton.gameObject.SetActive(false);
                nyaSettingsButton.gameObject.SetActive(false);

                BeatSaberMarkupLanguage.BeatSaberUI.SetImage(nyaImage, "Nya.Resources.Rainbow_Dance.gif");
                timeTweeningManager.KillAllTweens(floatingScreen);
                var positionTween = new FloatTween(0f, 1f, val => floatingScreen.gameObject.transform.position = Vector3.Lerp(floatingScreen.gameObject.transform.position, new Vector3(0f, 3f, 8f), val), 861.7f, EaseType.Linear, 1.621f);
                var rotationTween = new FloatTween(0f, 1f, val => floatingScreen.gameObject.transform.rotation = Quaternion.Lerp(floatingScreen.gameObject.transform.rotation, Quaternion.Euler(0f, 0f, 0f), val), 861.7f, EaseType.Linear, 1.621f);
                var theThing = floatingScreen.gameObject.transform.GetChild(0).GetComponent<ImageView>();
                var colorTween = new FloatTween(0f, 1f, val => theThing.color = Color.Lerp(Color.HSVToRGB(0f, 1f, 1f), Color.HSVToRGB(1f, 1f, 1f), val), 10f, EaseType.Linear);
                timeTweeningManager.AddTween(positionTween, floatingScreen);
                timeTweeningManager.AddTween(rotationTween, floatingScreen);
                colorTween.loop = true;
                timeTweeningManager.AddTween(colorTween, floatingScreen);
                return;
            }
            floatingScreen.gameObject.SetActive(false);
            floatingScreen.HandleReleased += FloatingScreen_HandleReleased;
            gamePause.didPauseEvent += GamePause_didPauseEvent;
            gamePause.willResumeEvent += GamePause_didResumeEvent;
        }

        public void Dispose()
        {
            gamePause.didPauseEvent -= GamePause_didPauseEvent;
            gamePause.willResumeEvent -= GamePause_didResumeEvent;
            floatingScreen.HandleReleased -= FloatingScreen_HandleReleased;
            floatingScreen.gameObject.SetActive(false);
            timeTweeningManager.KillAllTweens(floatingScreen);
        }

        private void GamePause_didPauseEvent() => floatingScreen.gameObject.SetActive(true);

        private void GamePause_didResumeEvent()
        {
            if (autoNyaToggle)
            {
                autoNyaToggle = false;
                nyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f);
                nyaButton.interactable = true;
            }
            settingsModalGameController.HideModal();
            floatingScreen.gameObject.SetActive(false);
        }

        private void FloatingScreen_HandleReleased(object sender, FloatingScreenHandleEventArgs args)
        {
            if (PluginConfig.Instance.SeperatePositions)
            {
                PluginConfig.Instance.PausePosition = floatingScreen.transform.position;
                PluginConfig.Instance.PauseRotation = floatingScreen.transform.eulerAngles;
            }
            else
            {
                PluginConfig.Instance.MenuPosition = floatingScreen.transform.position;
                PluginConfig.Instance.MenuRotation = floatingScreen.transform.eulerAngles;
            }
        }
    }
}