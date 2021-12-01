using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using IPA.Utilities;
using Nya.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nya.UI.ViewControllers
{
    public class SettingsModalMenuController : SettingsModalController
    {
        private readonly MainFlowCoordinator mainFlowCoordinator;

        public SettingsModalMenuController(NsfwConfirmModalController nsfwConfirmModalController, SettingsViewController settingsViewController, UIUtils uiUtils, MainFlowCoordinator mainFlowCoordinator) : base(nsfwConfirmModalController, settingsViewController, uiUtils)
        {
            this.mainFlowCoordinator = mainFlowCoordinator;
        }
        
        protected void Parse(Transform parentTransform)
        {
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.SettingsModal.bsml"), parentTransform.gameObject, this);
            ModalView.SetField("_animateParentCanvas", true);

            ApiDropDownTransform.Find("DropdownTableView").GetComponent<ModalView>().SetField("_animateParentCanvas", false);
            SfwDropDownTransform.Find("DropdownTableView").GetComponent<ModalView>().SetField("_animateParentCanvas", false);
            NsfwDropDownTransform.Find("DropdownTableView").GetComponent<ModalView>().SetField("_animateParentCanvas", false);

            //MoreSettingsTab.IsVisible = false;
        }

        public void ShowModal(Transform parentTransform)
        {
            Parse(parentTransform);
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");

            var root = parentTransform.root;
            if (root.name == "NyaMenuFloatingScreen" || root.name == "NyaGameFloatingScreen")
            {
                foreach (HoverHint hoverComponent in root.gameObject.GetComponentsInChildren<HoverHint>())
                {
                    hoverComponent.enabled = false;
                }
            }

            if (ImageUtils.nyaImageURL.EndsWith(".gif") || ImageUtils.nyaImageURL.EndsWith(".apng"))
            {
                NyaCopyButton.interactable = false;
            }
            else
            {
                NyaCopyButton.interactable = true;
            }
        }

        [UIAction("show-nya-settings")]
        private void ShowNyaSettings()
        {
            if (settingsViewController.isActiveAndEnabled) return;
            ReflectionUtil.InvokeMethod<object, FlowCoordinator>(mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf(), "PresentViewController", settingsViewController, null, ViewController.AnimationDirection.Vertical, false);
        }
    }
}
