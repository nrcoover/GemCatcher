using System.Threading.Tasks;
using System.Linq;
using Godot;

public partial class Game : Node2D
{
	const int DEFAULT_POINT_VALUE = 1;

	[Export] private PackedScene _gemSpawner;
	[Export] private Camera _camera;

	[Export] private AudioStreamPlayer _music;
	[Export] private AudioStreamPlayer _audioExplosion;
	[Export] private AudioStreamPlayer2D _audioCommanderEncouragement;
	[Export] private AudioStreamPlayer2D _audioCommanderAdvanceStage1;
	[Export] private AudioStreamPlayer2D _audioCommanderAdvanceStage2;
	[Export] private AudioStreamPlayer2D _audioCommanderAdvanceStage3;
	[Export] private AudioStreamPlayer _audioCommencingMission;
	[Export] private AudioStreamPlayer _audioMissionFailure;
	[Export] private AudioStreamPlayer _audioStageAdvancement;
	[Export] private AudioStreamPlayer2D _scoreSound;
	[Export] private AudioStreamPlayer2D _hurtSound;
	[Export] private AudioStreamPlayer2D _healthIncreaseSound;

	[Export] private int _shakeIntensity;
	[Export] private float _shakeTime;

	private Tween _musicVolumeTween;

	private bool _isDying = false;
	private bool _isVictorious = false;
	private float _musicDefaultVolume;

	public override async void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GameManager.Instance.ResetGame();
		SubscribeToSignals();
		InitializeVariables();
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
		GameManager.Instance.ResetGame();
	}

  private void InitializeVariables()
  {
    _musicDefaultVolume = _music.VolumeDb;
  }



#region Signals
	
	private void SubscribeToSignals()
	{
		SignalManager.Instance.InitiateDeathSequence += OnInitiateDeathSequenceAsync;
		SignalManager.Instance.InitiateVictorySequence += OnInitiateVictorySequenceAsync;
		SignalManager.Instance.Scored += OnScored;
		SignalManager.Instance.GemOffScreen += OnGemOffScreen;
		SignalManager.Instance.PlayerHurt += OnPlayerHurt;
		SignalManager.Instance.HealthRecovered += OnHealthRecovered;
		SignalManager.Instance.PowerUpSpawned += OnPowerUpSpawned;
		SignalManager.Instance.PowerUpRemoved += OnPowerUpRemoved;
		SignalManager.Instance.AdvanceStage += OnAdvanceStageAsync;
	}

  private void UnsubscribeFromSignals() {
		SignalManager.Instance.InitiateDeathSequence -= OnInitiateDeathSequenceAsync;
		SignalManager.Instance.InitiateVictorySequence -= OnInitiateVictorySequenceAsync;
		SignalManager.Instance.Scored -= OnScored;
		SignalManager.Instance.GemOffScreen -= OnGemOffScreen;
		SignalManager.Instance.PlayerHurt -= OnPlayerHurt;
		SignalManager.Instance.HealthRecovered -= OnHealthRecovered;
		SignalManager.Instance.PowerUpSpawned -= OnPowerUpSpawned;
		SignalManager.Instance.PowerUpRemoved -= OnPowerUpRemoved;
		SignalManager.Instance.AdvanceStage -= OnAdvanceStageAsync;
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

	private async void OnInitiateVictorySequenceAsync()
	{
		if (_isVictorious)
		{
			return;
		}

		_isVictorious = true;
	}

	private void OnScored(Color color)
	{
		if (_isDying)
		{
			return;
		}

		GameManager.Instance.IncrementScore(DEFAULT_POINT_VALUE);
		_scoreSound.Play();
	}
	
  private void OnPlayerHurt()
  {
    IncurDamage();
  }

	private void OnHealthRecovered()
  {
		if (_isDying)
		{
			return;
		}

		_healthIncreaseSound.Play();
  }

	private void OnGemOffScreen()
	{
		GameManager.Instance.IncrementMissedGems();
		SignalManager.Instance.EmitPlayerHurt();
	}

  private void OnPowerUpSpawned()
	{
		DuckMusicVolume();
	}

  private void OnPowerUpRemoved()
	{
		ResetMusicVolume();
	}
	
	public async void OnAdvanceStageAsync()
	{
		if (_isDying || !IsInsideTree())
		{
			return;
		}

		await PlayStageAdvancementSequenceAsync();

		// The player may have died while the sequence was awaiting.
		if (_isDying || !IsInsideTree())
		{
			return;
		}

		var isOddNumberedStage = GameManager.Instance.CurrentStage % 2 != 0;
		var isFirstStage = GameManager.Instance.CurrentStage == 1;
		if (isOddNumberedStage && !isFirstStage) 
		{
			InstantiateAdditionalGemSpawner();
		}
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

	private void DuckMusicVolume()
	{
		CreateMusicVolumeTween();
	}
	
	private void ResetMusicVolume()
	{
		_music.VolumeDb = _musicDefaultVolume;
	}

	private void PlayStageAdvancementCommanderAudio()
	{
		var randomNumber = Helper.GetRandomInt(1, 3);

		switch (randomNumber)
		{
			case 1:
				_audioCommanderAdvanceStage1.Play();
				break;
			case 2:
				_audioCommanderAdvanceStage2.Play();
				break;
			case 3:
				_audioCommanderAdvanceStage3.Play();
				break;
		}
	}
	
#endregion



#region Tweens

	private void KillAllTweens()
	{
		_musicVolumeTween?.Kill();
	}

	private void CreateMusicVolumeTween()
	{
		_musicVolumeTween = CreateTween();

		var tweenTime = .75f;
		var duckingPercentage = 1.35f;
		var adjustedVolume = _musicDefaultVolume * duckingPercentage;

		_musicVolumeTween.TweenProperty(
			_music,
			"volume_db",
			adjustedVolume,
			tweenTime
		).SetTrans(Tween.TransitionType.Cubic)
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

	private async Task PlayStageAdvancementSequenceAsync()
	{
    if (_isDying)
    {
			return;
    }

    _audioStageAdvancement.Play();

    await CreateTimerAsync(1.75f);

    if (_isDying || !IsInsideTree())
    {
			return;
    }

		PlayStageAdvancementCommanderAudio();
	}

	private async Task HandleDeathSequenceAudioAsync()
	{
		ScoreManager.Instance.HighScore = GameManager.Instance.CurrentScore;

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

	private void InstantiateAdditionalGemSpawner()
	{
		var additionalSpawnerTimeMultiplier = 7.0f;
		var spawner = (GemSpawner)_gemSpawner.Instantiate();

		spawner.SpawnTime *= additionalSpawnerTimeMultiplier
			* GameManager.Instance.CurrentStage;

		CallDeferred("add_child", spawner);

		GD.Print("Spawner SpawnTime = " + spawner.SpawnTime);
	}

#endregion

}
