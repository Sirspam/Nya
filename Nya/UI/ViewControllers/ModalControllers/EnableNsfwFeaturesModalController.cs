using System;
using System.Globalization;
using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static SiraUtil.Extras.Utilities;
using Component = UnityEngine.Component;

namespace Nya.UI.ViewControllers.ModalControllers
{
	internal class EnableNsfwFeaturesModalController
	{
		private enum FadeOutContent
		{
			HornyPastryPuffer,
			IncorrectMath,
			Blank
		}

		private struct ModalContent
		{
			internal readonly string TopText;
			internal readonly string MidText;
			internal readonly string MidImagePath;
			internal readonly string NoButtonText;
			internal readonly string YesButtonText;
			internal readonly bool ButtonIntractabilityCooldown;
			internal bool Animated;
			internal readonly bool ShowInputs;
			internal readonly bool MathTime;

			public ModalContent(string topText, string midText, string midImagePath, string noButtonText, string yesButtonText, bool buttonIntractabilityCooldown, bool animated, bool showInputs = true, bool mathTime = false)
			{
				TopText = topText;
				MidText = midText;
				MidImagePath = midImagePath;
				NoButtonText = noButtonText;
				YesButtonText = yesButtonText;
				ButtonIntractabilityCooldown = buttonIntractabilityCooldown;
				Animated = animated;
				ShowInputs = showInputs;
				MathTime = mathTime;
				
			}
		}

		private readonly ModalContent[] _modalContents =
		{
			new ModalContent("Woah There!", "Are you sure you want to enable NSFW features? You have to be 18+ to do this!", "Nya.Resources.Chocola_Surprised.png", "No", "Yes, I'm 18+", true, false),
			new ModalContent("Are you sure?", "Are you reallllyyyy certain that your age is 18 or above?", "Nya.Resources.Chocola_Question_Mark.png", "No, I'm not", "Yes, I'm certain", true, true),
			new ModalContent("Are you very very sure?", "If you're lying I will find out and tell your parents. \r\n(They will be very disappointed with you)", "Nya.Resources.Chocola_Angry.png", "Sorry, I lied", "Yes, I'm not lying", true, true),
			new ModalContent("Just double checking", "Okay so you're absolutely positive that you're 18+ and want to enable NSFW features?", "Nya.Resources.Chocola_Howdidyoudothat.png", "No", "Yes", true, true),
			new ModalContent("Surprise math question!", "To confirm that you're really 18, let's do a maths question! \r\nWhat is 6 + 9 * (4 - 2) + 0?", "Nya.Resources.Chocola_Laugh.png", "No", "Yes", true, true, true, true),
			new ModalContent("Correct!", "If you got that right then you must be a smart and sensible adult!", "Nya.Resources.Chocola_Happy.png", "but I'm not...", "I am! 😃", true, true),
			new ModalContent("Wait!", "The NSFW features could be dangerous! \r\nWhy else would they be called Not Safe For Work??", "Nya.Resources.Chocola_Spooked.png", "That sounds risky!", "I am prepared", true, true),
			new ModalContent("Last time I'll ask", "So you definitely want to enable NSFW and suffer the consequences which may entail from it?", "Nya.Resources.Chocola_Bashful.png", "No", "Yes", true, true)
		};

		private bool _parsed;

		private int _confirmationStage;

		private Action? _yesButtonPressedCallback;

		private PanelAnimationSO _presentPanelAnimation = null!;

		private PanelAnimationSO _dismissPanelAnimation = null!;
		
		private TimeTweeningManager _timeTweeningManager = null!;

		[Inject]
		private void Construct(TimeTweeningManager timeTweeningManager)
		{
			_timeTweeningManager = timeTweeningManager;
		}

		private int ConfirmationStage
		{
			get => _confirmationStage;
			set
			{
				_confirmationStage = value;
				
				if (value > _modalContents.Length - 1)
				{
					FadeOutModal(FadeOutContent.HornyPastryPuffer);
				}
				else
				{
					ChangeModalContent(_modalContents[value]);
				}
			}
		}
		
		[UIComponent("modal")]
		private readonly ModalView _modalView = null!;

		[UIObject("root-vertical")] 
		private readonly GameObject _rootVerticalGameObject = null!;
		
		[UIObject("main-layout")] 
		private readonly GameObject _mainLayoutGameObject = null!;
		
		[UIObject("buttons-layout")] 
		private readonly GameObject _buttonsLayoutGameObject = null!;
		
		[UIObject("slider-layout")] 
		private readonly GameObject _sliderLayoutGameObject = null!;
		
		[UIObject("horny-pastry-puffer-layout")] 
		private readonly GameObject _hornyPastryPufferLayout = null!;
		
		[UIComponent("top-text")]
		private readonly CurvedTextMeshPro _topText = null!;
		
		[UIComponent("mid-text")]
		private readonly CurvedTextMeshPro _midText = null!;
		
		[UIComponent("mid-image")]
		private readonly ImageView _midImage = null!;
		
		[UIComponent("no-button")]
		private readonly Button _noButton = null!;
		
		[UIComponent("yes-button")]
		private readonly Button _yesButton = null!;
		
		[UIComponent("slider")]
		private readonly SliderSetting _mathSlider = null!;
		
		[UIComponent("submit-button")]
		private readonly Button _submitButton = null!;
		
		[UIParams] 
		private readonly BSMLParserParams _parserParams = null!;

		private void Parse(Component parentTransform)
		{
			if (!_modalView && !_parsed)
			{
				BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "Nya.UI.Views.EnableNsfwFeaturesModalView.bsml"), parentTransform.gameObject, this);
				_modalView.name = "NyaEnableNsfwFeaturesModal";
				_presentPanelAnimation = _modalView.GetField<PanelAnimationSO, ModalView>("_presentPanelAnimations");
				_dismissPanelAnimation = _modalView.GetField<PanelAnimationSO, ModalView>("_dismissPanelAnimation");

				_parsed = true;
			}

			_mainLayoutGameObject.SetActive(true);
			_hornyPastryPufferLayout.SetActive(false);
			_buttonsLayoutGameObject.SetActive(true);
			_sliderLayoutGameObject.SetActive(false);
			ConfirmationStage = 0;
		}
		
		internal void ShowModal(Transform parentTransform, Action? yesButtonPressedCallback)
		{
			Parse(parentTransform);
			_parserParams.EmitEvent("close-modal");
			_parserParams.EmitEvent("open-modal");
			_yesButtonPressedCallback = yesButtonPressedCallback;
		}

		private void HideModal()
		{
			_parserParams.EmitEvent("close-modal");
		}

		private void FadeOutModal(FadeOutContent fadeOutContent)
		{
			FloatTween tween;
			Action? startAction = null;
			var modalCanvasGroup = _modalView.GetComponent<CanvasGroup>();
			switch (fadeOutContent)
			{
				case FadeOutContent.HornyPastryPuffer:
				{
					startAction = () =>
					{
						_yesButtonPressedCallback?.Invoke();
						_mainLayoutGameObject.SetActive(false);
						_hornyPastryPufferLayout.SetActive(true);
						
					};

					tween = new FloatTween(1f, 0f, val => modalCanvasGroup.alpha = val, 2f, EaseType.Linear, 2f);
					break;
				}
				case FadeOutContent.IncorrectMath:
				{
					startAction = () =>
					{
						_buttonsLayoutGameObject.SetActive(false);
						_sliderLayoutGameObject.SetActive(false);
						ChangeModalContent(new ModalContent("Incorrect!", "", "Nya.Resources.Chocola_Sobbing.png", "", "", false, false, false));
					};

					tween = new FloatTween(1f, 0f, val => modalCanvasGroup.alpha = val, 2f, EaseType.Linear, 2f);
					break;
				}
				default:
				case FadeOutContent.Blank:
				{
					startAction = () =>
					{
						_yesButtonPressedCallback?.Invoke();
						_mainLayoutGameObject.SetActive(false);
						_hornyPastryPufferLayout.SetActive(false);
					};
					
					tween = new FloatTween(1f, 0f, val => modalCanvasGroup.alpha = val, 2f, EaseType.Linear);
					break;
				}
			}

			tween.onCompleted = () => _modalView.Hide(false);

			async void FinishedCallback()
			{
				startAction?.Invoke();
				await PauseChamp;
				_presentPanelAnimation.ExecuteAnimation(_rootVerticalGameObject, () => _timeTweeningManager.AddTween(tween, _modalView));
			}
			
			_dismissPanelAnimation.ExecuteAnimation(_rootVerticalGameObject, FinishedCallback);
		}
		
		private void ChangeModalContent(ModalContent modalContent)
		{
			if (modalContent.Animated)
			{
				async void FinishedCallback()
				{
					modalContent.Animated = false;
					ChangeModalContent(modalContent);
					await PauseChamp;
					_presentPanelAnimation.ExecuteAnimation(_rootVerticalGameObject);
				}

				_dismissPanelAnimation.ExecuteAnimation(_rootVerticalGameObject, FinishedCallback);
				return;
			}
			
			_topText.text = modalContent.TopText;
			_midText.text = modalContent.MidText;
			_midImage.SetImage(modalContent.MidImagePath);

			if (modalContent.ShowInputs)
			{
				if (modalContent.MathTime)
				{
					_buttonsLayoutGameObject.SetActive(false);
					_sliderLayoutGameObject.SetActive(true);
					_mathSlider.Value = 0f;

					if (modalContent.ButtonIntractabilityCooldown)
					{
#if !DEBUG
					IntractabilityCooldown(_mathSlider);
					IntractabilityCooldown(_submitButton);
#endif
					}
				}
				else
				{
					_buttonsLayoutGameObject.SetActive(true);
					_sliderLayoutGameObject.SetActive(false);
				
					_noButton.SetButtonText(modalContent.NoButtonText);
					_yesButton.SetButtonText(modalContent.YesButtonText);

					if (modalContent.ButtonIntractabilityCooldown)
					{
#if !DEBUG
					IntractabilityCooldown(_noButton);
					IntractabilityCooldown(_yesButton);
#endif
					}
				}	
			}
		}

		private static async void IntractabilityCooldown(SliderSetting gameObject)
		{
			gameObject.interactable = false;
			await AwaitSleep(2500);
			gameObject.interactable = true;
		}

		private static async void IntractabilityCooldown(Selectable gameObject)
		{
			gameObject.interactable = false;
			await AwaitSleep(2500);
			gameObject.interactable = true;
		}
		
		[UIAction("yes-clicked")]
		private void YesClicked()
		{
			ConfirmationStage += 1;
		}

		[UIAction("no-clicked")]
		private void NoClicked()
		{
			HideModal();
		}
		
		[UIAction("submit-clicked")]
		private void SubmitClicked()
		{
			if (_mathSlider.Value.ToString(CultureInfo.InvariantCulture) == "24")
			{
				ConfirmationStage += 1;
			}
			else
			{
				FadeOutModal(FadeOutContent.IncorrectMath);
			}
		}
	}
}