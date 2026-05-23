using System.Collections.Generic;
using Godot;

public partial class MainMenuUi : Control
{
	[Export] TextureButton _playButton;
	[Export] TextureButton _rulesButton;
	[Export] TextureButton _movesButton;
	[Export] TextureButton _quitButton;
	[Export] TextureButton _creditsButton;

	[Export] MenuModal _movesModal;
	[Export] MenuModal _rulesModal;
	[Export] MenuModal _creditsModal;
	
	[Export] Label _titleLayer1;
	[Export] Label _titleLayer2;
	[Export] Label _titleLayer3;
	[Export] Label _titleLayer4;
	[Export] Label _titleLayer5;
	[Export] Label _titleLayer6;
	[Export] Timer _titleTimer;
	[Export] Control _titleContainer;

	private Dictionary<Label, Vector2> _baseScales = [];
	private Dictionary<Label, Color> _baseColors = [];

	private bool _isAnimatingTitle = false;
	private bool _isFirstAnimationRun = false;

	public override void _Ready()
	{
		_isFirstAnimationRun = true;

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
