using Godot;

public partial class MainMenuUi : Control
{
	[Export] TextureButton _playButton;
	[Export] TextureButton _quitButton;
	[Export] Label _titleLayer1;
	[Export] Label _titleLayer2;
	[Export] Label _titleLayer3;
	[Export] Label _titleLayer4;
	[Export] Label _titleLayer5;
	[Export] Label _titleLayer6;
	[Export] Timer _titleTimer;
	[Export] Control _titleContainer;

	private Tween _colorScaleTween;

	public override void _Ready()
	{
		_playButton.Pressed += OnPlayClicked;
		_quitButton.Pressed += OnQuitClicked;

		InitializeTitleColors();
		HandleLabelTweeningAsync();
		SubscribeToSignals();
	}

	public void SubscribeToSignals()
	{
		_titleTimer.Timeout += OnTitleTimeout;
	}

  private void OnTitleTimeout()
  {
    HandleLabelTweeningAsync();
  }

  private void InitializeTitleColors()
	{
		_titleLayer1.Modulate = new Color(Constants.CustomColors.PurplePastelle);
		_titleLayer2.Modulate = new Color(Constants.CustomColors.BluePastelle);
		_titleLayer3.Modulate = new Color(Constants.CustomColors.GreenPastelle);
		_titleLayer4.Modulate = new Color(Constants.CustomColors.YellowPastelle);
		_titleLayer5.Modulate = new Color(Constants.CustomColors.OrangePastelle);
		_titleLayer6.Modulate = new Color(Constants.CustomColors.RedPastelle);
	}
	
  private async void HandleLabelTweeningAsync()
  {
    var labels = _titleContainer.GetChildren();

		GD.Print($"Labels Variable: {labels}");

		if (labels.Count > 0)
		{
			foreach (Label label in labels) {
				CreateColorScaleTweenAsync(Colors.White, label);
				GD.Print($"Current Label: {label}");
				await ToSignal(_colorScaleTween, Tween.SignalName.Finished);
			}

			labels.RemoveAt(labels.Count - 1);
			labels.Reverse();
			
			foreach (Label label in labels) {
				CreateColorScaleTweenAsync(Colors.White, label);
				GD.Print($"Current Label: {label}");
				await ToSignal(_colorScaleTween, Tween.SignalName.Finished);
			}
		}
  }

  private void OnPlayClicked()
  {
    LevelManager.Instance.LoadGame();
  }

	private void OnQuitClicked()
  {
    LevelManager.Instance.QuitGame();
  }

	private async void CreateColorScaleTweenAsync(Color color, Label label)
	{
		var tweenTime = 0.05f;
		var originalScale = Scale;
		var scaleMultiplier = 1.01f;
		var originalColor = label.Modulate;

		_colorScaleTween = CreateTween();

		_colorScaleTween.SetParallel(true);

		_colorScaleTween.TweenProperty(
			label,
			PropertyName.Modulate.ToString(),
			color,
			tweenTime
		).SetTrans(Tween.TransitionType.Cubic)
		.SetEase(Tween.EaseType.Out);

		_colorScaleTween.TweenProperty(
			label,
			PropertyName.Scale.ToString(),
			originalScale * scaleMultiplier,
			tweenTime
		).SetTrans(Tween.TransitionType.Back)
		.SetEase(Tween.EaseType.Out);

		await ToSignal(_colorScaleTween, Tween.SignalName.Finished);

		_colorScaleTween = CreateTween();

		_colorScaleTween.SetParallel(true);

		_colorScaleTween.TweenProperty(
			label,
			PropertyName.Modulate.ToString(),
			originalColor,
			tweenTime
		).SetTrans(Tween.TransitionType.Cubic)
		.SetEase(Tween.EaseType.Out);

		_colorScaleTween.TweenProperty(
			label,
			PropertyName.Scale.ToString(),
			originalScale,
			tweenTime
		).SetTrans(Tween.TransitionType.Back)
		.SetEase(Tween.EaseType.Out);
	}
}
