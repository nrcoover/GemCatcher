using GemCatcher.utils;
using Godot;

public partial class GameUi : Control
{
	enum HealthStatus
	{
		FullHealth = 5,
		FleshWound = 4,
		Injured = 3,
		Hurt = 2,
		MortallyWounded = 1,
		Dead = 0
	}

	[Export] private Label _scoreLabel;
	[Export] private Label _customDebugLabel;
	
	[Export] private Node2D _heart1;
	[Export] private Node2D _heart2;
	[Export] private Node2D _heart3;
	[Export] private Node2D _heart4;
	[Export] private Node2D _heart5;

	[Export] private AnimationPlayer _animator;

	private Tween _colorScaleTween;
	private Tween _heartScaleTween;
	private bool _isDying = false;


	public override void _Ready()
	{
		if (CustomDebug.IsDebugging)
		{
			_customDebugLabel.Visible = CustomDebug.IsDebugging;
			UpdateDebugLabel();
		}

		SubscribeToSignals();
		ResetUi();
		InitializeScoreUi();
	}

	public override void _Process(double delta)
	{
		if (CustomDebug.IsDebugging)
		{
			UpdateDebugLabel();
		}
	}

  public override void _ExitTree()
  {
    UnsubscribeFromSignals();
  }



#region Signals

	private void SubscribeToSignals()
	{
		SignalManager.Instance.InitiateDeathSequence += OnInitiateDeathSequenceAsync;
		SignalManager.Instance.HealthRecovered += OnHealthRecovered;
		SignalManager.Instance.ScoreUpdated += OnScoreUpdated;
		SignalManager.Instance.PlayerHurt += OnPlayerHurt;
		SignalManager.Instance.GameVictory += OnGameVictory;
		SignalManager.Instance.ResetGame += OnGameReset;
	}

  private void UnsubscribeFromSignals()
	{
		SignalManager.Instance.InitiateDeathSequence -= OnInitiateDeathSequenceAsync;
		SignalManager.Instance.HealthRecovered -= OnHealthRecovered;
		SignalManager.Instance.ScoreUpdated -= OnScoreUpdated;
		SignalManager.Instance.PlayerHurt -= OnPlayerHurt;
		SignalManager.Instance.GameVictory -= OnGameVictory;
		SignalManager.Instance.ResetGame -= OnGameReset;
	}

  private async void OnInitiateDeathSequenceAsync()
	{
		// TODO: Consider refactoring _isDying into GameManager enum GameState
		if (_isDying)
		{
			return;
		}

		_isDying = true;

		KillAllTweens();
	}

	private void OnHealthRecovered()
  {
		if (_isDying)
		{
			return;
		}

    UpdateHealthUi();
  }

	private void OnScoreUpdated(Color color)
	{
		if (_isDying)
		{
			return;
		}

		UpdateScoreUi(color);
	}

	private void OnPlayerHurt()
  {
		UpdateHealthUi();
  }
	
  private void OnGameVictory()
  {
    _animator.Play(Constants.Animations.GameUi.Fadeout);
  }

	private void OnGameReset()
	{
		ResetUi();
	}

#endregion



#region UI

	private void ResetUi()
	{
		_animator.Play(Constants.Animations.GameUi.Reset);
	}

	private void InitializeScoreUi()
	{
		_scoreLabel.Text = $"Score: {GameManager.Instance.CurrentScore:000}";
	}
	
	private void UpdateScoreUi(Color color)
	{
		if (GameManager.Instance.CurrentGameState == (int)GameManager.GameState.VICTORY)
		{
			return;
		}

		_scoreLabel.Text = $"Score: {GameManager.Instance.CurrentScore:000}";

		var scaleMultiplier = 1.10f;
		_scoreLabel.SelfModulate = Colors.White;
		_scoreLabel.Scale = Vector2.One * scaleMultiplier;

		CreateColorScaleTween(color);
	}

  private void UpdateHealthUi()
	{
		if (GameManager.Instance.CurrentGameState == (int)GameManager.GameState.VICTORY)
		{
			return;
		}

		var currentHealth = GameManager.Instance.GetHealth();

		switch (currentHealth)
		{
			case (int)HealthStatus.FullHealth:
				_heart1.Visible = true;
				_heart2.Visible = true;
				_heart3.Visible = true;
				_heart4.Visible = true;
				_heart5.Visible = true;
				CreateHeartScaleTween(_heart1);
				
				break;
			case (int)HealthStatus.FleshWound:
				_heart1.Visible = false;
				_heart2.Visible = true;
				_heart3.Visible = true;
				_heart4.Visible = true;
				_heart5.Visible = true;
				CreateHeartScaleTween(_heart2);

				break;
			case (int)HealthStatus.Injured:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = true;
				_heart4.Visible = true;
				_heart5.Visible = true;
				CreateHeartScaleTween(_heart3);

				break;
			case (int)HealthStatus.Hurt:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = false;
				_heart4.Visible = true;
				_heart5.Visible = true;
				CreateHeartScaleTween(_heart4);

				break;
			case (int)HealthStatus.MortallyWounded:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = false;
				_heart4.Visible = false;
				_heart5.Visible = true;
				CreateHeartScaleTween(_heart5);

				break;
			default:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = false;
				_heart4.Visible = false;
				_heart5.Visible = false;
				break;
		}
	}
	
#endregion



#region Tweens

	private void KillAllTweens()
	{
		_colorScaleTween?.Kill();
		_heartScaleTween?.Kill();
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

	private void CreateHeartScaleTween(Node2D heart)
  {
		_heartScaleTween = CreateTween();

    var scaleMultiplier = 3.25f;
    var tweenTime = 0.35f;
		var originalScale = heart.Scale;
		var originalColor = heart.Modulate;

    heart.Modulate = IncreaseColorIntensity(originalColor);
    heart.Scale = Vector2.One * scaleMultiplier;

		_heartScaleTween.SetParallel(true);

		_heartScaleTween.TweenProperty(
			heart,
			PropertyName.Modulate.ToString(),
			originalColor,
			tweenTime
		).SetTrans(Tween.TransitionType.Cubic)
		.SetEase(Tween.EaseType.Out);

		_heartScaleTween.TweenProperty(
			heart,
			PropertyName.Scale.ToString(),
			originalScale,
			tweenTime
		).SetTrans(Tween.TransitionType.Back)
		.SetEase(Tween.EaseType.Out);
  }

	private Color IncreaseColorIntensity(Color color)
	{
		float newIntensity = 3.25f;

		return Color.FromHsv(
				color.H,
				color.S,
				newIntensity,
				color.A
		);
	}

#endregion

	private void UpdateDebugLabel()
	{
		_customDebugLabel.Text = 
			$"Game State: {CustomDebug.GetGameStateString()}"
			+ $"\nGame Mode: {CustomDebug.GetGameModeString()}"
			+ $"\nCurrent Stage: {GameManager.Instance.CurrentStage}"
			+ $"\nDifficulty Level: {GameManager.Instance.DifficultyLevel}";
	}
}
