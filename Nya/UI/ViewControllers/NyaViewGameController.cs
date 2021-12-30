using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using System;
using Tweening;
using UnityEngine;
using Zenject;

namespace Nya.UI.ViewControllers
{
    internal class NyaViewGameController : NyaViewController, IInitializable, IDisposable
    {
        private readonly UIUtils _uiUtils;
        private readonly SettingsModalGameController settingsModalGameController;
        private readonly IGamePause gamePause;
        private readonly TimeTweeningManager timeTweeningManager;
        private FloatingScreen floatingScreen;

        public NyaViewGameController(PluginConfig config, ImageUtils imageUtils, UIUtils uiUtils, SettingsModalGameController settingsModalGameController, IGamePause gamePause, TimeTweeningManager timeTweeningManager) : base(config, imageUtils)
        {
            _uiUtils = uiUtils;
            this.settingsModalGameController = settingsModalGameController;
            this.gamePause = gamePause;
            this.timeTweeningManager = timeTweeningManager;
        }

        public void Initialize()
        {
            if (Config.SeperatePositions)
                floatingScreen = _uiUtils.CreateNyaFloatingScreen(this, Config.PausePosition, Quaternion.Euler(Config.PauseRotation));
            else
                floatingScreen = _uiUtils.CreateNyaFloatingScreen(this, Config.MenuPosition, Quaternion.Euler(Config.MenuRotation));
            floatingScreen.gameObject.name = "NyaGameFloatingScreen";

            // Wanted to do a wacky easter egg for my beloved shiny happy days map but it prooved to be too much of a hassle
            // Leaving this commented in case I come back to it in the future
            //
            //if (Config.EasterEggs && beatmap.level.levelID == "custom_level_69E494F4A295197BF03720029086FABE6856FBCE") // e970 my beloved
            //{
            //    floatingScreen.handle.SetActive(false);
            //    nyaButton.gameObject.SetActive(false);
            //    nyaAutoButton.gameObject.SetActive(false);
            //    nyaSettingsButton.gameObject.SetActive(false);

            //    BeatSaberMarkupLanguage.BeatSaberUI.SetImage(nyaImage, "Nya.Resources.Rainbow_Dance.gif");
            //    timeTweeningManager.KillAllTweens(floatingScreen);
            //    var positionTween = new FloatTween(0f, 1f, val => floatingScreen.gameObject.transform.position = Vector3.Lerp(floatingScreen.gameObject.transform.position, new Vector3(0f, 3f, 8f), val), 861.7f, EaseType.Linear, 1.621f);
            //    var rotationTween = new FloatTween(0f, 1f, val => floatingScreen.gameObject.transform.rotation = Quaternion.Lerp(floatingScreen.gameObject.transform.rotation, Quaternion.Euler(0f, 0f, 0f), val), 861.7f, EaseType.Linear, 1.621f);
            //    var theThing = floatingScreen.gameObject.transform.GetChild(0).GetComponent<ImageView>();
            //    var colorTween = new FloatTween(0f, 1f, val => theThing.color = Color.Lerp(Color.HSVToRGB(0f, 1f, 1f), Color.HSVToRGB(1f, 1f, 1f), val), 10f, EaseType.Linear);
            //    timeTweeningManager.AddTween(positionTween, floatingScreen);
            //    timeTweeningManager.AddTween(rotationTween, floatingScreen);
            //    colorTween.loop = true;
            //    timeTweeningManager.AddTween(colorTween, floatingScreen);
            //    return;
            //}
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
            settingsModalGameController.HideModal();
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
            if (Config.SeperatePositions)
            {
                Config.PausePosition = floatingScreen.transform.position;
                Config.PauseRotation = floatingScreen.transform.eulerAngles;
            }
            else
            {
                Config.MenuPosition = floatingScreen.transform.position;
                Config.MenuRotation = floatingScreen.transform.eulerAngles;
            }
        }

        [UIAction("settings-button-clicked")]
        protected void SettingsButtonClicked()
        {
            if (autoNyaToggle)
            {
                AutoNya();
            }
            settingsModalGameController.ShowModal(settingsButtonTransform);
        }
    }
}