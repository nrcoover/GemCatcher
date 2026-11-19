using System.Collections.Generic;
using Godot;

// TODO: Fix mouse not working bug
public partial class MainMenuUi : Control
{
	[Export] private Control _mainMenuControls;
	[Export] private TextureButton _playButton;
	[Export] private TextureButton _infiniteModeButton;
	[Export] private TextureButton _rulesButton;
	[Export] private TextureButton _movesButton;
	[Export] private TextureButton _quitButton;
	[Export] private TextureButton _creditsButton;
	[Export] private TextureButton _resetHighScoreButton;
	[Export] private TextureButton _restoreHighScoreButton;
	[Export] private GemButton _helloButton;
	[Export] private GemButton _thankYouButton;

	[Export] private Control _modalControls;
	[Export] private MenuModal _movesModal;
	[Export] private MenuModal _rulesModal;
	[Export] private MenuModal _creditsModal;
	[Export] private MenuModal _helloModal;
	
	[Export] private Label _titleLayer1;
	[Export] private Label _titleLayer2;
	[Export] private Label _titleLayer3;
	[Export] private Label _titleLayer4;
	[Export] private Label _titleLayer5;
	[Export] private Label _titleLayer6;
	[Export] private Label _highScoreLabel;
	[Export] private Label _stagesWonLabel;

	[Export] private Timer _titleTimer;
	[Export] private Control _titleContainer;
	[Export] private HBoxContainer _highScoreContainer;

	[Export] private AudioStreamPlayer _thankYouAudio;

	private Dictionary<Label, Vector2> _baseScales = [];
	private Dictionary<Label, Color> _baseColors = [];

	private bool _isAnimatingTitle = false;
	private bool _isFirstAnimationRun = false;

	private Color _thankYouButtonOriginalColor;

	private Control _lastFocusedControl;
	private readonly Dictionary<Control, Control.FocusModeEnum> _savedFocusModes = new();
	private MenuModal _activeModal;

	public override void _Ready()
	{
		SubscribeToSignals();

		UIInputManager.Instance.SetCursorModeOverride(null);
		OnInputModeChanged(UIInputManager.Instance.CurrentMode);

		_isFirstAnimationRun = true;

		if (SaveDataManager.Instance.IsInfiniteModeUnlocked)
		{
			ShowInfiniteModeButton();
		}
		else
		{
			HideInfiniteModeButton();
		}

		HandleLabelDisplays();
		CloseAllModals();
		InitializeTitleColors();
		HandleLabelTweeningAsync();
		SetThankYouButtonOriginalColor();
		SetThankYouButtonVisibility();
		SetHelloButtonVisibility();
		ResetThankYouButton();
	}

  public override void _UnhandledInput(InputEvent @event)
	{
		if (_activeModal != null)
		{
			return;
		}

		if (UIInputManager.Instance.CurrentMode != UIInputManager.InputMode.Gamepad)
		{
			return;
		}

		bool navigationPressed = @event.IsActionPressed("ui_up")
			|| @event.IsActionPressed("ui_down") 
			|| @event.IsActionPressed("ui_left") 
			|| @event.IsActionPressed("ui_right") 
			|| @event.IsActionPressed("ui_focus_prev") 
			|| @event.IsActionPressed("ui_focus_next");

		if (navigationPressed && !IsAnyControlFocused())
		{
			_playButton.CallDeferred(Control.MethodName.GrabFocus);
		}
	}

  public override void _ExitTree()
  {
    UnsubscribeFromSignals();
  }

  #region Signals

  public void SubscribeToSignals()
	{
		SignalManager.Instance.SaveDataReset += OnSaveDataReset;
		UIInputManager.Instance.InputModeChanged += OnInputModeChanged;
		_titleTimer.Timeout += OnTitleTimeout;
		_movesModal.Closed += OnModalClosed;
		_rulesModal.Closed += OnModalClosed;
		_creditsModal.Closed += OnModalClosed;
		_helloModal.Closed += OnModalClosed;
		_playButton.Pressed += OnPlayClicked;
		_rulesButton.Pressed += OnRulesClicked;
		_movesButton.Pressed += OnMovesClicked;
		_quitButton.Pressed += OnQuitClicked;
		_creditsButton.Pressed += OnCreditsClicked;
		_resetHighScoreButton.Pressed += OnResetHighScoreClicked;
		_restoreHighScoreButton.Pressed += OnRestoreHighScoreClicked;
		_infiniteModeButton.Pressed += OnInfiniteModeClicked;
		_helloButton.Pressed += OnHelloClicked;
		_thankYouButton.Pressed += OnThankYouClicked;
		_thankYouAudio.Finished += OnThankYouAudioFinished;
	}

	public void UnsubscribeFromSignals()
	{
		SignalManager.Instance.SaveDataReset -= OnSaveDataReset;
		UIInputManager.Instance.InputModeChanged -= OnInputModeChanged;
	}

  private void OnSaveDataReset()
  {
    HandleLabelDisplays();
		HandleInfiniteModeButtonDisplay();
  }

	private void OnInputModeChanged(UIInputManager.InputMode mode)
	{
		if (mode == UIInputManager.InputMode.MouseKeyboard)
		{
			ClearAllFocus();
			return;
		}

		if (_activeModal != null)
		{
			_activeModal.GrabInitialFocus();
			return;
		}

		if (!IsAnyControlFocused())
		{
			_playButton.CallDeferred(Control.MethodName.GrabFocus);
		}
	}

  private void OnTitleTimeout()
  {
    HandleLabelTweeningAsync();
  }

	private void OnModalClosed(MenuModal modal)
	{
		if (_activeModal != modal)
		{
			return;
		}

		_activeModal = null;

		SetControlsFocusEnabled(_mainMenuControls, true);

		if (UIInputManager.Instance.CurrentMode == UIInputManager.InputMode.Gamepad)
		{
			RestorePreviousFocus();
		}
		else
		{
			ClearAllFocus();
		}
	}

#endregion


#region Button Signals

	private void OnPlayClicked()
  {
		GameManager.Instance.SetGameModeNormal();
		SaveDataManager.Instance.MarkGameAsPlayed();
		LevelManager.Instance.LoadGame();
  }

  private void OnRulesClicked()
	{
		OpenModal(_rulesModal);
	}

  private void OnMovesClicked()
	{
		OpenModal(_movesModal);
	}
	
	private void OnQuitClicked()
  {
    LevelManager.Instance.QuitGame();
  }

  private void OnCreditsClicked()
	{
		OpenModal(_creditsModal);
	}
	
  private void OnResetHighScoreClicked()
  {
		CloseAllModals();
		GameManager.Instance.SetGameModeNormal();
		SaveDataManager.Instance.ResetSaveData();
  }

  private void OnRestoreHighScoreClicked()
  {
		CloseAllModals();
    SaveDataManager.Instance.RestoreHighScore();
  }

	private void OnInfiniteModeClicked()
	{
		GameManager.Instance.SetGameModeInfinite();
		LevelManager.Instance.LoadGame();
	}

	private void OnHelloClicked()
	{
		OpenModal(_helloModal);
	}

	private void OnThankYouClicked()
	{
		CloseAllModals();
		_thankYouButton.Disabled = true;
		_thankYouButton.KillSpinTwin();

		_thankYouButton.SelfModulate = new Color(
			Helper.GetColorFromHexCode(Constants.CustomColors.DisabledGray)
		);

		_thankYouAudio.Play();
	}

	private void OnThankYouAudioFinished()
	{
		ResetThankYouButton();
	}

#endregion

	private void CloseAllModals()
	{
		_rulesModal.Visible = false;
		_movesModal.Visible = false;
		_creditsModal.Visible = false;
		_helloModal.Visible = false;
	}

	private void InitializeTitleColors()
	{
		StoreLabelDefaults(_titleLayer1, Constants.CustomColors.PurplePastelle);
		StoreLabelDefaults(_titleLayer2, Constants.CustomColors.BluePastelle);
		StoreLabelDefaults(_titleLayer3, Constants.CustomColors.GreenPastelle);
		StoreLabelDefaults(_titleLayer4, Constants.CustomColors.YellowPastelle);
		StoreLabelDefaults(_titleLayer5, Constants.CustomColors.OrangePastelle);
		StoreLabelDefaults(_titleLayer6, Constants.CustomColors.RedPastelle);
	}

	private void StoreLabelDefaults(Label label, string colorHex)
	{
		label.Modulate = new Color(colorHex);

		_baseScales[label] = label.Scale;
		_baseColors[label] = label.Modulate;
	}
	
  private void HandleLabelDisplays()
  {
		_highScoreContainer.Visible = false;
    var highScore = SaveDataManager.Instance.HighScore;
		var highScoreRestore = SaveDataManager.Instance.HighScoreRestore;
		int stagesWon = highScore / GameManager.STAGE_ADVANCEMENT_INCREMENTOR;

		if (highScore != 0 || highScoreRestore != 0)
		{
			_highScoreContainer.Visible = true;
			_highScoreLabel.Text = $"High Score: {highScore}";
			_stagesWonLabel.Text = $"Stages Won: {stagesWon}";
			_stagesWonLabel.Visible = SaveDataManager.Instance.IsInfiniteModeUnlocked;

			if (highScoreRestore == 0)
			{
				_restoreHighScoreButton.Visible = false;
			}
			else
			{
				_restoreHighScoreButton.Visible = true;
			}
		}
  }

	private void HandleInfiniteModeButtonDisplay()
	{
		if(SaveDataManager.Instance.IsInfiniteModeUnlocked)
		{
			return;
		}

		HideInfiniteModeButton();
	}

	private void HideInfiniteModeButton()
	{
		_infiniteModeButton.Hide();
	}

	private void ShowInfiniteModeButton()
	{
		_infiniteModeButton.Show();
	}

	private void SetThankYouButtonOriginalColor()
	{
		_thankYouButtonOriginalColor = _thankYouButton.SelfModulate;
	}

	private void SetThankYouButtonVisibility()
	{
		_thankYouButton.Visible = SaveDataManager.Instance.IsInfiniteModeUnlocked;
	}

	private void SetHelloButtonVisibility()
	{
		_helloButton.Visible = SaveDataManager.Instance.HasPlayedGame;
	}

	private void ResetThankYouButton()
	{
		_thankYouButton.Disabled = false;
		_thankYouButton.SelfModulate = _thankYouButtonOriginalColor;
	}
	
  private async void HandleLabelTweeningAsync()
	{
		if (_isFirstAnimationRun)
		{
			await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
			_isFirstAnimationRun = false;
			_titleTimer.Start();
		}
		
		if (_isAnimatingTitle) return;

		_isAnimatingTitle = true;

		var labels = _titleContainer.GetChildren();

		float delayBetweenLabels = 0.16f;

		foreach (Node node in labels)
		{
			if (node is Label label)
			{
				CreateColorScaleTween(label);

				await ToSignal(
					GetTree().CreateTimer(delayBetweenLabels),
					SceneTreeTimer.SignalName.Timeout
				);
			}
		}

		var reverseLabels = new Godot.Collections.Array<Node>(labels);
		reverseLabels.RemoveAt(reverseLabels.Count - 1);
		reverseLabels.Reverse();

		foreach (Node node in reverseLabels)
		{
			if (node is Label label)
			{
				CreateColorScaleTween(label);

				await ToSignal(
					GetTree().CreateTimer(delayBetweenLabels),
					SceneTreeTimer.SignalName.Timeout
				);
			}
		}

		_isAnimatingTitle = false;
	}

	private void CreateColorScaleTween(Label label)
	{
		float tweenTime = 0.22f;
		float scaleMultiplier = 1.015f;

		Vector2 originalScale = _baseScales[label];
		Color originalColor = _baseColors[label];
		Color flashColor = originalColor.Lightened(0.45f);

		Tween tween = CreateTween();

		tween.SetParallel(true);

		tween.TweenProperty(
			label,
			PropertyName.Modulate.ToString(),
			flashColor,
			tweenTime
		).SetTrans(Tween.TransitionType.Sine)
		.SetEase(Tween.EaseType.Out);

		tween.TweenProperty(
			label,
			PropertyName.Scale.ToString(),
			originalScale * scaleMultiplier,
			tweenTime
		).SetTrans(Tween.TransitionType.Sine)
		.SetEase(Tween.EaseType.Out);

		tween.SetParallel(false);

		tween.TweenProperty(
			label,
			PropertyName.Modulate.ToString(),
			originalColor,
			tweenTime
		).SetTrans(Tween.TransitionType.Sine)
		.SetEase(Tween.EaseType.InOut);

		tween.Parallel().TweenProperty(
			label,
			PropertyName.Scale.ToString(),
			originalScale,
			tweenTime
		).SetTrans(Tween.TransitionType.Sine)
		.SetEase(Tween.EaseType.InOut);
	}

#region Handle UI

	private void HandleFocusControl()
	{
		_playButton.CallDeferred(MethodName.GrabFocus);
	}

	private bool IsAnyControlFocused()
	{
		Control focusedControl = GetViewport().GuiGetFocusOwner();
		return focusedControl != null;
	}

	private void ClearAllFocus()
	{
		GetViewport().GuiReleaseFocus();
	}

	public void OpenModal(MenuModal modal)
	{
		if (_activeModal != null)
		{
			return;
		}

		_lastFocusedControl = GetViewport().GuiGetFocusOwner();

		_activeModal = modal;

		SetControlsFocusEnabled(_mainMenuControls, false);

		modal.Open();
	}

	private void SetControlsFocusEnabled(Node root, bool enabled)
	{
		foreach (Node child in root.GetChildren())
		{
			if (child is Control control)
			{
				if (!enabled)
				{
					if (control.FocusMode != Control.FocusModeEnum.None)
					{
						_savedFocusModes[control] = control.FocusMode;
						control.FocusMode = Control.FocusModeEnum.None;
					}
				}
				else if (_savedFocusModes.TryGetValue(control, out var originalMode))
				{
					control.FocusMode = originalMode;
					_savedFocusModes.Remove(control);
				}
			}

			if (child.GetChildCount() > 0)
			{
				SetControlsFocusEnabled(child, enabled);
			}
		}
	}

	private void RestorePreviousFocus()
	{
		if (CanReceiveFocus(_lastFocusedControl))
		{
			_lastFocusedControl.CallDeferred(Control.MethodName.GrabFocus);
		}
		else
		{
			_playButton.CallDeferred(Control.MethodName.GrabFocus);
		}

		_lastFocusedControl = null;
	}

	private bool CanReceiveFocus(Control control)
	{
		if (control == null || !GodotObject.IsInstanceValid(control)
			|| (!control.IsVisibleInTree())
			|| (control.FocusMode == Control.FocusModeEnum.None)
			|| (control is BaseButton button && button.Disabled)
		)
		{
			return false;
		}

		return true;
	}

#endregion
}
