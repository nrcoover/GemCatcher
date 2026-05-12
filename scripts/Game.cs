using System;
using Godot;

public partial class Game : Node2D
{
	const int DEFAULT_POINT_VALUE = 1;

	enum HealthStatus
	{
		FullHealth = 3,
		Injured = 2,
		MortallyWounded = 1,
		Dead = 0
	}

	[Export] private Camera _camera;
	[Export] private Label _scoreLabel;

	[Export] private AudioStreamPlayer _explosion;
	[Export] private AudioStreamPlayer2D _scoreSound;
	[Export] private AudioStreamPlayer2D _hurtSound;

	[Export] private Node2D _heart1;
	[Export] private Node2D _heart2;
	[Export] private Node2D _heart3;

	[Export] private int _shakeIntensity;
	[Export] private float _shakeTime;

	private Tween _colorScaleTween;

	private int _score = 0;

	public override void _Ready()
	{
		SubscribeToSignals();
	}

	 public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("exit"))
		{
			HandleEscape();
		}
	}

	public override void _ExitTree()
	{
		UnsubscribeFromSignals();
	}

	public async void OnInitiateDeathSequenceAsync()
	{
		_colorScaleTween.Kill();

		GetTree().Paused = true;
		await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
		_explosion.Play();
		await ToSignal(GetTree().CreateTimer(1.5f), SceneTreeTimer.SignalName.Timeout);
		
		GameManager.Instance.ResetGame();
		LevelManager.Instance.LoadMainMenu();
	}

	private void SubscribeToSignals()
	{
		SignalManager.Instance.InitiateDeathSequence += OnInitiateDeathSequenceAsync;
		SignalManager.Instance.Scored += OnScored;
		SignalManager.Instance.GemOffScreen += OnGemOffScreen;
	}

	private void UnsubscribeFromSignals() {
		SignalManager.Instance.InitiateDeathSequence -= OnInitiateDeathSequenceAsync;
		SignalManager.Instance.Scored -= OnScored;
		SignalManager.Instance.GemOffScreen -= OnGemOffScreen;
	}

	private void HandleEscape()
	{
		if (Input.IsKeyPressed(Key.Escape))
		{
			LevelManager.Instance.LoadMainMenu();
		}
	}

	private void OnScored(Color color)
	{
		IncrementScore(DEFAULT_POINT_VALUE);
		_scoreSound.Play();
		UpdateScoreUi(color);
	}

	private void UpdateScoreUi(Color color)
	{
		_scoreLabel.Text = $"Score: {_score:000}";

		var scaleMultiplier = 1.10f;
		_scoreLabel.SelfModulate = Colors.White;
		_scoreLabel.Scale = Vector2.One * scaleMultiplier;

		CreateColorScaleTween(color);
	}

  private void CreateColorScaleTween(Color color)
  {
    _colorScaleTween = CreateTween();
		var tweenTime = 0.35f;

		_colorScaleTween.SetParallel(true);

		_colorScaleTween.TweenProperty(
			_scoreLabel,
			PropertyName.SelfModulate.ToString(),
			color,
			tweenTime
		).SetTrans(Tween.TransitionType.Cubic)
		.SetEase(Tween.EaseType.Out);

		_colorScaleTween.TweenProperty(
			_scoreLabel,
			PropertyName.Scale.ToString(),
			Vector2.One,
			tweenTime
		).SetTrans(Tween.TransitionType.Back)
		.SetEase(Tween.EaseType.Out);
  }

  private void UpdateHealthUi()
	{
		var currentHealth = GameManager.Instance.GetHealth();

		switch (currentHealth)
		{
			case (int)HealthStatus.FullHealth:
				_heart1.Visible = true;
				_heart2.Visible = true;
				_heart3.Visible = true;
				break;
			case (int)HealthStatus.Injured:
				_heart1.Visible = false;
				_heart2.Visible = true;
				_heart3.Visible = true;
				break;
			case (int)HealthStatus.MortallyWounded:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = true;
				break;
			case (int)HealthStatus.Dead:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = false;
				break;
			default:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = false;
				break;
		}
	}

	private void OnGemOffScreen()
	{
		GameManager.Instance.IncrementMissedGems();
		_hurtSound.Play();
		_camera.ScreenShake(_shakeIntensity, _shakeTime);
		UpdateHealthUi();
	}

	private void IncrementScore(int points)
	{
		_score += points;
	}
}
