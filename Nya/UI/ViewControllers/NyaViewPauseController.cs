using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using Nya.Configuration;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRUIControls;
using Zenject;

namespace Nya.UI.ViewControllers
{
    internal class NyaViewPauseController : NyaViewController, IInitializable, IDisposable
    {
        private FloatingScreen floatingScreen;
        private PauseController pauseController;
        private PauseMenuManager pauseMenuManager;


        private HoverHintController hoverHintController;

        public NyaViewPauseController(PauseController pauseController, SettingsModalController settingsModalController) : base(settingsModalController)
        {
            this.pauseController = pauseController;
        }
        public void Initialize()
        {
            floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(90, 70), PluginConfig.Instance.showHandle, PluginConfig.Instance.pausePosition, Quaternion.Euler(PluginConfig.Instance.pauseRotation), curvatureRadius: 220, true);
            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.NyaView.bsml"), floatingScreen.gameObject, this);
            if (PluginConfig.Instance.showHandle)
            {
                floatingScreen.handle.transform.localPosition = new Vector3(0f, -floatingScreen.ScreenSize.y / 1.8f, -5f);
                floatingScreen.handle.transform.localScale = new Vector3(floatingScreen.ScreenSize.x * 0.8f, floatingScreen.ScreenSize.y / 15f, floatingScreen.ScreenSize.y / 15f);
                floatingScreen.handle.gameObject.layer = 5;
                floatingScreen.HandleReleased += FloatingScreen_HandleReleased;
            }
            floatingScreen.gameObject.name = "NyaPauseFloatingScreen";
            floatingScreen.gameObject.layer = 5;


            if (!PluginConfig.Instance.showHandle && !Resources.FindObjectsOfTypeAll<FirstPersonFlyingController>().FirstOrDefault())
            {
                // yoinked from slice details https://github.com/ckosmic/SliceDetails/blob/master/SliceDetails/UI/UICreator.cs#L61
                // Screen mover needs to be completely remade for dragging to work in-game
                pauseMenuManager = Resources.FindObjectsOfTypeAll<PauseMenuManager>().First();
                VRPointer gameVRPointer = pauseMenuManager.transform.Find("Wrapper/MenuWrapper/EventSystem").GetComponent<VRPointer>();
                UnityEngine.Object.Destroy(floatingScreen.screenMover);
                floatingScreen.screenMover = gameVRPointer.gameObject.AddComponent<FloatingScreenMoverPointer>();
                floatingScreen.screenMover.Init(floatingScreen, gameVRPointer);
            }
            floatingScreen.gameObject.SetActive(false);
            pauseController.didPauseEvent += PauseController_didPauseEvent;
            pauseController.didResumeEvent += PauseController_didResumeEvent;
        }

        public void Dispose()
        {
            pauseController.didPauseEvent -= PauseController_didPauseEvent;
            pauseController.didResumeEvent -= PauseController_didResumeEvent;
        }

        private void PauseController_didPauseEvent()
        {
            floatingScreen.gameObject.SetActive(true);
        }

        private void PauseController_didResumeEvent()
        {
            if (autoNyaToggle)
            {
                autoNyaToggle = false;
                nyaAutoButton.gameObject.transform.Find("Underline").gameObject.GetComponent<ImageView>().color = new Color(1f, 1f, 1f, 0.502f);
                nyaButton.interactable = true;
            }
            floatingScreen.gameObject.SetActive(false);
        }

        private void FloatingScreen_HandleReleased(object sender, FloatingScreenHandleEventArgs args)
        {
            PluginConfig.Instance.pausePosition = floatingScreen.transform.position;
            PluginConfig.Instance.pauseRotation = floatingScreen.transform.eulerAngles;
        }
    }
}
