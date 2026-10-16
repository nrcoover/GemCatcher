using System.Threading.Tasks;
using System.Linq;
using Godot;
using GemCatcher.utils;

public partial class Game : Node2D
{
	const int DEFAULT_POINT_VALUE = 1;

	[Export] private PackedScene _gemSpawner;
	[Export] private Camera _camera;

	// Music
	[Export] private AudioStreamPlayer _music;
	[Export] private AudioStreamPlayer _musicReversed;

	// SFX
	[Export] private AudioStreamPlayer _audioExplosion;
	[Export] private AudioStreamPlayer2D _scoreSound;
	[Export] private AudioStreamPlayer2D _hurtSound;
	[Export] private AudioStreamPlayer2D _healthIncreaseSound;

	// Commander Dialogue
	[Export] private AudioStreamPlayer2D _audioCommanderEncouragement;
	[Export] private AudioStreamPlayer2D _audioCommanderAdvanceStage1;
	[Export] private AudioStreamPlayer2D _audioCommanderAdvanceStage2;
	[Export] private AudioStreamPlayer2D _audioCommanderAdvanceStage3;
	[Export] private AudioStreamPlayer2D _audioCommanderCongratulations;

	// Computer Dialogue
	[Export] private AudioStreamPlayer _audioCommencingMission;
	[Export] private AudioStreamPlayer _audioMissionFailure;
	[Export] private AudioStreamPlayer _audioStageAdvancement;
	[Export] private AudioStreamPlayer _audioFinalStageAdvancement;
	[Export] private AudioStreamPlayer _audioMissionComplete;


	[Export] private Node2D _normalSpawnerContainer;
	[Export] private Node2D _victorySpawnerContainer;

	[Export] private GameSuccessScreen _gameVictoryScreen;

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

		if (@event.IsActionPressed("test") && CustomDebug.IsDebugging)
		{
			GameManager.Instance.SetGameStateVictory();
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
		SignalManager.Instance.Scored += OnScored;
		SignalManager.Instance.GemOffScreen += OnGemOffScreen;
		SignalManager.Instance.PlayerHurt += OnPlayerHurt;
		SignalManager.Instance.HealthRecovered += OnHealthRecovered;
		SignalManager.Instance.PowerUpSpawned += OnPowerUpSpawned;
		SignalManager.Instance.PowerUpRemoved += OnPowerUpRemoved;
		SignalManager.Instance.AdvanceStage += OnAdvanceStageAsync;
		SignalManager.Instance.GameVictory += OnGameVictory;
	}

  private void UnsubscribeFromSignals() {
		SignalManager.Instance.InitiateDeathSequence -= OnInitiateDeathSequenceAsync;
		SignalManager.Instance.Scored -= OnScored;
		SignalManager.Instance.GemOffScreen -= OnGemOffScreen;
		SignalManager.Instance.PlayerHurt -= OnPlayerHurt;
		SignalManager.Instance.HealthRecovered -= OnHealthRecovered;
		SignalManager.Instance.PowerUpSpawned -= OnPowerUpSpawned;
		SignalManager.Instance.PowerUpRemoved -= OnPowerUpRemoved;
		SignalManager.Instance.AdvanceStage -= OnAdvanceStageAsync;
		SignalManager.Instance.GameVictory -= OnGameVictory;
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

	private async void InitiateVictorySequenceAsync()
	{
		if (_isVictorious)
		{
			return;
		}

		_isVictorious = true;

		// Play winning celebration sound
		_music.Stop();
		_audioExplosion.Play(); // TODO: Replace with special gem explosion sound
		_musicReversed.Play();

		await CreateTimerAsync(0.75f);

		_audioMissionComplete.Play();

		await CreateTimerAsync(2.5f);

		_audioCommanderCongratulations.Play();

		await CreateTimerAsync(2.0f);

		// Victory screen initates with fade-in animation
		_gameVictoryScreen.Visible = true;

		SignalManager.Instance.EmitInitiateVictorySequence();
	}

	private void OnScored(Color color)
	{
		if (_isDying)
		{
			return;
		}

		GameManager.Instance.IncrementScore(DEFAULT_POINT_VALUE, color);
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
		if (GameManager.Instance.CurrentGameState == (int)GameManager.GameState.VICTORY)
		{
			return;
		}

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

	private void OnGameVictory()
	{
		InitiateVictorySequenceAsync();
	}

#endregion



#region Input

  private void HandleEscape()
	{
		if (Input.IsKeyPressed(Key.Escape))
		{
			GameManager.Instance.ResetGame();
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
		// TODO: Prevent same audio twice in a row by storing previous random number in variable and re-roll if same as previous.
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

		if (GameManager.Instance.CurrentStage == GameManager.VICTORY_STAGE)
		{
			_audioFinalStageAdvancement.Play();
		}
		else
		{
    	_audioStageAdvancement.Play();
		}

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

		_normalSpawnerContainer.CallDeferred("add_child", spawner);

		GD.Print("Spawner SpawnTime = " + spawner.SpawnTime);
	}

#endregion

}
