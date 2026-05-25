using System.Threading.Tasks;
using System.Linq;
using Godot;

public partial class Game : Node2D
{
	const int DEFAULT_POINT_VALUE = 1;

	enum HealthStatus
	{
		FullHealth = 5,
		FleshWound = 4,
		Injured = 3,
		Hurt = 2,
		MortallyWounded = 1,
		Dead = 0
	}

	[Export] private Camera _camera;
	[Export] private Label _scoreLabel;

	[Export] private AudioStreamPlayer _audioExplosion;
	[Export] private AudioStreamPlayer2D _audioCommanderEncouragement;
	[Export] private AudioStreamPlayer _audioCommencingMission;
	[Export] private AudioStreamPlayer _audioMissionFailure;
	[Export] private AudioStreamPlayer2D _scoreSound;
	[Export] private AudioStreamPlayer2D _hurtSound;

	[Export] private Node2D _heart1;
	[Export] private Node2D _heart2;
	[Export] private Node2D _heart3;
	[Export] private Node2D _heart4;
	[Export] private Node2D _heart5;

	[Export] private int _shakeIntensity;
	[Export] private float _shakeTime;

	private Tween _colorScaleTween;

	private int _score = 0;
	private bool _isDying = false;

	public override async void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GameManager.Instance.ResetGame();
		SubscribeToSignals();
		await PlayGameStartSequenceAsync();
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



#region Signals
	
	private void SubscribeToSignals()
	{
		SignalManager.Instance.InitiateDeathSequence += OnInitiateDeathSequenceAsync;
		SignalManager.Instance.Scored += OnScored;
		SignalManager.Instance.GemOffScreen += OnGemOffScreen;
		SignalManager.Instance.PlayerHurt += OnPlayerHurt;
	}

  private void UnsubscribeFromSignals() {
		SignalManager.Instance.InitiateDeathSequence -= OnInitiateDeathSequenceAsync;
		SignalManager.Instance.Scored -= OnScored;
		SignalManager.Instance.GemOffScreen -= OnGemOffScreen;
		SignalManager.Instance.PlayerHurt -= OnPlayerHurt;
	}

	public async void OnInitiateDeathSequenceAsync()
	{
		if (_isDying)
		{
			return;
		}

		_isDying = true;

		KillAllTweens();
		StopMoveableObjectProcessing();
		StopAllAudio();
		
		await PlayDeathCameraShakeAsync();

		if (!IsInsideTree())
		{
			return;
		}

		await HandleDeathSequenceAudioAsync();

		if (!IsInsideTree())
		{
			return;
		}
	}

	private void OnScored(Color color)
	{
		IncrementScore(DEFAULT_POINT_VALUE);
		_scoreSound.Play();
		UpdateScoreUi(color);
	}
	
  private void OnPlayerHurt()
  {
    IncurDamage();
  }

	private void OnGemOffScreen()
	{
		GameManager.Instance.IncrementMissedGems();
		SignalManager.Instance.EmitPlayerHurt();
	}

#endregion



#region Input

	private void HandleEscape()
	{
		if (Input.IsKeyPressed(Key.Escape))
		{
			LevelManager.Instance.LoadMainMenu();
		}
	}
	
#endregion
	


#region UI

	private void UpdateScoreUi(Color color)
	{
		_scoreLabel.Text = $"Score: {_score:000}";

		var scaleMultiplier = 1.10f;
		_scoreLabel.SelfModulate = Colors.White;
		_scoreLabel.Scale = Vector2.One * scaleMultiplier;

		CreateColorScaleTween(color);
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
				_heart4.Visible = true;
				_heart5.Visible = true;
				break;
			case (int)HealthStatus.FleshWound:
				_heart1.Visible = false;
				_heart2.Visible = true;
				_heart3.Visible = true;
				_heart4.Visible = true;
				_heart5.Visible = true;
				break;
			case (int)HealthStatus.Injured:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = true;
				_heart4.Visible = true;
				_heart5.Visible = true;
				break;
			case (int)HealthStatus.Hurt:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = false;
				_heart4.Visible = true;
				_heart5.Visible = true;
				break;
			case (int)HealthStatus.MortallyWounded:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = false;
				_heart4.Visible = false;
				_heart5.Visible = true;
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



#region Audio

	private void StopAllAudio()
	{
		var audioStreams = Helper.GetAllObjectsInGroup(
			GetTree().Root,
			Constants.GroupNames.AudioStreams
		);

		foreach (Node audio in audioStreams)
		{
			if (audio.Name == "Explosion" || audio.Name == "HurtSound")
			{
				continue;
			}

			if (audio is AudioStreamPlayer player)
			{
				player.Stop();
			}
			else if (audio is AudioStreamPlayer2D player2D)
			{
				player2D.Stop();
			}
		}
	}
	
#endregion



#region Tweens

	private void KillAllTweens()
	{
		if (_colorScaleTween != null)
		{
			_colorScaleTween.Kill();
		}
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
	
#endregion



#region Asynchronous Tasks

	private async Task CreateTimerAsync(float timeInSeconds)
	{
		var tree = GetTree();

		if (tree == null)
		{
			return;
		}

		await ToSignal(
			tree.CreateTimer(timeInSeconds),
			SceneTreeTimer.SignalName.Timeout
		);
	}
	
	private async Task PlayGameStartSequenceAsync()
  {
		_audioCommencingMission.Play();

		await CreateTimerAsync(1.5f);

		_audioCommanderEncouragement.Play();
  }

	private async Task PlayDeathCameraShakeAsync()
	{
		var shakeTime = 2.0f;
		var minShakeIntensity = _shakeIntensity * 0.1f;
		var maxShakeIntensity = _shakeIntensity * 3;

		_camera.RampScreenShake(shakeTime, minShakeIntensity, maxShakeIntensity);

		await CreateTimerAsync(shakeTime);
	}

	private async Task HandleDeathSequenceAudioAsync()
	{
		ScoreManager.Instance.HighScore = _score;

		SignalManager.Instance.EmitShowGameOverScreen();

		_audioExplosion.Play();

		await CreateTimerAsync(1.5f);

		SignalManager.Instance.EmitShowMissionFailurePanel();

		_audioMissionFailure.Play();

		await CreateTimerAsync(2.5f);

		SignalManager.Instance.EmitShowGameOverButtons();
	}

#endregion



#region Other
	
	private void IncurDamage()
	{
		_hurtSound.Play();
		_camera.ScreenShake(_shakeIntensity, _shakeTime);
		UpdateHealthUi();
	}

	private void IncrementScore(int points)
	{
		_score += points;
	}

	private void StopMoveableObjectProcessing()
	{
		var moveables = Helper.GetAllObjectsInGroup(
			GetTree().Root,
			Constants.GroupNames.MoveableObjects
		);

		foreach (Node2D moveable in moveables.Cast<Node2D>())
		{
			moveable.ProcessMode = ProcessModeEnum.Disabled;
		}
	}

#endregion

}
