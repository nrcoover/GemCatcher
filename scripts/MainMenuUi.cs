using System.Collections.Generic;
using Godot;

public partial class MainMenuUi : Control
{
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

	public override void _Ready()
	{
		SubscribeToSignals();

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

  public override void _ExitTree()
  {
    UnsubscribeFromSignals();
  }

  #region Signals

  public void SubscribeToSignals()
	{
		SignalManager.Instance.SaveDataReset += OnSaveDataReset;
		_titleTimer.Timeout += OnTitleTimeout;
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
	}

  private void OnSaveDataReset()
  {
    HandleLabelDisplays();
		HandleInfiniteModeButtonDisplay();
  }

  private void OnTitleTimeout()
  {
    HandleLabelTweeningAsync();
  }

	private void OnPlayClicked()
  {
		GameManager.Instance.SetGameModeNormal();
		SaveDataManager.Instance.MarkGameAsPlayed();
		LevelManager.Instance.LoadGame();
  }

  private void OnRulesClicked()
	{
		CloseAllModals();
		_rulesModal.Visible = true;
	}

  private void OnMovesClicked()
	{
		CloseAllModals();
		_movesModal.Visible = true;
	}
	
	private void OnQuitClicked()
  {
    LevelManager.Instance.QuitGame();
  }

  private void OnCreditsClicked()
	{
		CloseAllModals();
		_creditsModal.Visible = true;
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
		CloseAllModals();
		_helloModal.Visible = true;
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
}
