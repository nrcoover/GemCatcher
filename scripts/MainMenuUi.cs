using System.Collections.Generic;
using Godot;

public partial class MainMenuUi : Control
{
	[Export] private TextureButton _playButton;
	[Export] private TextureButton _rulesButton;
	[Export] private TextureButton _movesButton;
	[Export] private TextureButton _quitButton;
	[Export] private TextureButton _creditsButton;
	[Export] private TextureButton _resetHighScoreButton;
	[Export] private TextureButton _restoreHighScoreButton;

	[Export] private MenuModal _movesModal;
	[Export] private MenuModal _rulesModal;
	[Export] private MenuModal _creditsModal;
	
	[Export] private Label _titleLayer1;
	[Export] private Label _titleLayer2;
	[Export] private Label _titleLayer3;
	[Export] private Label _titleLayer4;
	[Export] private Label _titleLayer5;
	[Export] private Label _titleLayer6;
	[Export] private Label _highScoreLabel;

	[Export] private Timer _titleTimer;
	[Export] private Control _titleContainer;
	[Export] private MarginContainer _marginHighScoreContainer;

	private Dictionary<Label, Vector2> _baseScales = [];
	private Dictionary<Label, Color> _baseColors = [];

	private bool _isAnimatingTitle = false;
	private bool _isFirstAnimationRun = false;

	public override void _Ready()
	{
		_isFirstAnimationRun = true;

		HandleHighScoreDisplay();
		CloseAllModals();
		InitializeTitleColors();
		HandleLabelTweeningAsync();
		SubscribeToSignals();
	}

  #region Signals

  public void SubscribeToSignals()
	{
		_titleTimer.Timeout += OnTitleTimeout;
		_playButton.Pressed += OnPlayClicked;
		_rulesButton.Pressed += OnRulesClicked;
		_movesButton.Pressed += OnMovesClicked;
		_quitButton.Pressed += OnQuitClicked;
		_creditsButton.Pressed += OnCreditsClicked;
		_resetHighScoreButton.Pressed += OnResetHighScoreClicked;
		_restoreHighScoreButton.Pressed += OnRestoreHighScoreClicked;
	}

  private void OnTitleTimeout()
  {
    HandleLabelTweeningAsync();
  }

	private void OnPlayClicked()
  {
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

  private void OnResetHighScoreClicked()
  {
    ScoreManager.Instance.ResetHighScore();
  }

  private void OnRestoreHighScoreClicked()
  {
    ScoreManager.Instance.RestoreHighScore();
  }

  private void OnCreditsClicked()
	{
		CloseAllModals();
		_creditsModal.Visible = true;
	}

#endregion

	private void CloseAllModals()
	{
		_rulesModal.Visible = false;
		_movesModal.Visible = false;
		_creditsModal.Visible = false;
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
	
  private void HandleHighScoreDisplay()
  {
		_marginHighScoreContainer.Visible = false;
    var highScore = ScoreManager.Instance.HighScore;
		var highScoreRestore = ScoreManager.Instance.HighScoreRestore;

		if (highScore != 0 || highScoreRestore != 0)
		{
			_marginHighScoreContainer.Visible = true;
			_highScoreLabel.Text = $"High Score: {highScore}";

			if (highScore == 0)
			{
				_restoreHighScoreButton.Visible = false;
			}
			else
			{
				_restoreHighScoreButton.Visible = true;
			}
		}
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
