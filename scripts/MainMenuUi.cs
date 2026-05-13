using System.Threading.Tasks;
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

		if (labels.Count == 0) return;

		foreach (Node node in labels)
		{
			if (node is Label label)
			{
				await CreateColorScaleTweenAsync(Colors.White, label);
			}
		}

		labels.RemoveAt(labels.Count - 1);
		labels.Reverse();

		foreach (Node node in labels)
		{
			if (node is Label label)
			{
				await CreateColorScaleTweenAsync(Colors.White, label);
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

	private async Task CreateColorScaleTweenAsync(Color color, Label label)
	{
		var tweenTime = 0.03f;
		var originalScale = label.Scale;
		var scaleMultiplier = 1.01f;
		var originalColor = label.Modulate;

		var tween = CreateTween();

		tween.SetParallel(true);

		tween.TweenProperty(
			label,
			PropertyName.Modulate.ToString(),
			color,
			tweenTime
		);

		tween.TweenProperty(
			label,
			PropertyName.Scale.ToString(),
			originalScale * scaleMultiplier,
			tweenTime
		);

		await ToSignal(tween, Tween.SignalName.Finished);

		tween = CreateTween();

		tween.SetParallel(true);

		tween.TweenProperty(
			label,
			PropertyName.Modulate.ToString(),
			originalColor,
			tweenTime
		);

		tween.TweenProperty(
			label,
			PropertyName.Scale.ToString(),
			originalScale,
			tweenTime
		);

		await ToSignal(tween, Tween.SignalName.Finished);
	}
}
