using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Godot;

public partial class Game : Node2D
{
	const int DEFAULT_POINT_VALUE = 1;
	const int STREAK_BONUS_INTERVAL = 10;
	const int STREAK_BONUS_POINTS = 5;
	const int MIN_VISIBLE_STREAK = 2;
	const int STAGE_CLEAR_BONUS_POINTS = 3;
	const int METEOR_DODGE_POINTS = 2;
	const int METEOR_UNLOCK_STAGE = 3;
	const int STARDUST_POINT_VALUE = 3;
	const float ADDITIONAL_SPAWNER_BASE_WAIT_TIME = 9.0f;
	const float ADDITIONAL_SPAWNER_MINIMUM_WAIT_TIME = 4.0f;
	const float ADDITIONAL_SPAWNER_STAGE_REDUCTION = 0.5f;
	const int MAX_HEART_SLOTS = 10;

	[Export] private PackedScene _gemSpawner;

	[Export] private Camera _camera;
	[Export] private Label _scoreLabel;
	[Export] private Label _eventLabel;
	[Export] private Label _meteorStormLabel;

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
	[Export] private AudioStreamPlayer2D _stardustShowerSound;

	[Export] private Node _heartContainer;
	[Export] private Control _actionSlot;
	[Export] private TextureRect _actionIcon;
	[Export] private Label _actionStatusLabel;
	[Export] private ColorRect _nukeFlash;

	[Export] private int _shakeIntensity;
	[Export] private float _shakeTime;

	private Tween _colorScaleTween;
	private Tween _heartScaleTween;
	private Tween _musicVolumeTween;
	private Tween _eventLabelTween;
	private Tween _actionSlotTween;
	private Tween _nukeFlashTween;

	private int _score = 0;
	private int _catchStreak = 0;
	private bool _isDying = false;
	private float _musicDefaultVolume;
	private float _meteorStormTimeRemaining;
	private readonly List<Node2D> _heartSlots = [];
	private readonly List<Node2D> _heartFills = [];

	public override async void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GameManager.Instance.ResetGame();
		SubscribeToSignals();
		InitializeVariables();
		await PlayGameStartSequenceAsync();
	}

	public override void _Process(double delta)
	{
		if (!_meteorStormLabel.Visible)
		{
			return;
		}

		_meteorStormTimeRemaining = Mathf.Max(
			0.0f,
			_meteorStormTimeRemaining - (float)delta
		);
		UpdateMeteorStormCountdown();
	}

  public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("use_action"))
		{
			TryDetonateNuke();
			GetViewport().SetInputAsHandled();
			return;
		}

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
		_catchStreak = 0;
		_eventLabel.Visible = false;
		_meteorStormLabel.Visible = false;
		InitializeHeartUi();
		UpdateHealthUi(false);
		UpdateActionSlotUi(GameManager.Instance.HasNuke, false);
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
		SignalManager.Instance.MeteorHit += OnMeteorHit;
		SignalManager.Instance.MeteorDodged += OnMeteorDodged;
		SignalManager.Instance.StardustShowerStarted += OnStardustShowerStarted;
		SignalManager.Instance.StardustCollected += OnStardustCollected;
		SignalManager.Instance.NukeSlotChanged += OnNukeSlotChanged;
		SignalManager.Instance.MeteorStormStarted += OnMeteorStormStarted;
		SignalManager.Instance.MeteorStormEnded += OnMeteorStormEnded;
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
		SignalManager.Instance.MeteorHit -= OnMeteorHit;
		SignalManager.Instance.MeteorDodged -= OnMeteorDodged;
		SignalManager.Instance.StardustShowerStarted -= OnStardustShowerStarted;
		SignalManager.Instance.StardustCollected -= OnStardustCollected;
		SignalManager.Instance.NukeSlotChanged -= OnNukeSlotChanged;
		SignalManager.Instance.MeteorStormStarted -= OnMeteorStormStarted;
		SignalManager.Instance.MeteorStormEnded -= OnMeteorStormEnded;
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
		IncrementCatchStreak();
		IncrementScore(GetCatchScoreValue());
		_scoreSound.Play();
		UpdateScoreUi(color);
	}
	
  private void OnPlayerHurt()
  {
    IncurDamage();
  }

	private void OnGemOffScreen()
	{
		ResetCatchStreak();
		GameManager.Instance.IncrementMissedGems();
		SignalManager.Instance.EmitPlayerHurt();
	}
	
  private void OnHealthRecovered()
  {
		_healthIncreaseSound.Play();
    UpdateHealthUi();
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
		ApplyStageClearReward();
		ShowMeteorStageMessage();

		await PlayStageAdvancementSequenceAsync();

		if (!IsActiveInTree())
		{
			return;
		}

    var spawner = (GemSpawner)_gemSpawner.Instantiate();
		spawner.SpawnTime = Mathf.Max(
			ADDITIONAL_SPAWNER_MINIMUM_WAIT_TIME,
			ADDITIONAL_SPAWNER_BASE_WAIT_TIME
				- GameManager.Instance.CurrentStage * ADDITIONAL_SPAWNER_STAGE_REDUCTION
		);
		spawner.ConfigureAsAdditionalSpawner();
		CallDeferred("add_child", spawner);
		GD.Print("Additional spawner wait time = " + spawner.SpawnTime);
  }

	private void OnMeteorHit()
	{
		ResetCatchStreak();
		GameManager.Instance.DecrementHealth();
		ShowEventMessage("METEOR IMPACT", new Color(Constants.CustomColors.RedBright), 0.8f);
		SignalManager.Instance.EmitPlayerHurt();
	}

	private void OnMeteorDodged()
	{
		var meteorColor = new Color(Constants.CustomColors.OrangeBright);

		IncrementScore(METEOR_DODGE_POINTS);
		UpdateScoreUi(meteorColor);
		ShowEventMessage($"METEOR DODGED +{METEOR_DODGE_POINTS}", meteorColor, 0.8f);
	}

	private void OnStardustShowerStarted()
	{
		_stardustShowerSound.Play();
		ShowEventMessage(
			"STARDUST SHOWER",
			new Color(Constants.CustomColors.YellowBright),
			1.5f
		);
	}

	private void OnStardustCollected(Color color)
	{
		IncrementScore(STARDUST_POINT_VALUE);
		_scoreSound.Play();
		UpdateScoreUi(color);
		ShowEventMessage($"STARDUST +{STARDUST_POINT_VALUE}", color, 0.55f);
	}

	private void OnNukeSlotChanged(bool hasNuke)
	{
		UpdateActionSlotUi(hasNuke);

		if (hasNuke)
		{
			ShowEventMessage(
				"NUKE READY - PRESS SPACE",
				new Color(Constants.CustomColors.PinkBright),
				1.1f
			);
		}
	}

	private void OnMeteorStormStarted(float duration)
	{
		_meteorStormTimeRemaining = duration;
		_meteorStormLabel.Visible = true;
		UpdateMeteorStormCountdown();
		ClearFallingObjects(Constants.GroupNames.CollectibleGems);
		ShowEventMessage(
			"METEOR STORM - DODGE!",
			new Color(Constants.CustomColors.RedBright),
			1.5f
		);
	}

	private void OnMeteorStormEnded()
	{
		_meteorStormTimeRemaining = 0.0f;
		_meteorStormLabel.Visible = false;
		ShowEventMessage(
			"METEOR STORM CLEARED",
			new Color(Constants.CustomColors.GreenBright),
			1.2f
		);
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
		UpdateScoreText();

		var scaleMultiplier = 1.10f;
		_scoreLabel.SelfModulate = Colors.White;
		_scoreLabel.Scale = Vector2.One * scaleMultiplier;

		CreateColorScaleTween(color);
	}

	private void InitializeHeartUi()
	{
		_heartSlots.Clear();
		_heartFills.Clear();

		for (var heartNumber = 1; heartNumber <= MAX_HEART_SLOTS; heartNumber++)
		{
			var slot = _heartContainer.GetNode<Node2D>($"Heart{heartNumber}Faded");
			var fill = slot.GetNode<Node2D>($"Heart{heartNumber}Full");
			_heartSlots.Add(slot);
			_heartFills.Add(fill);
		}
	}

	private void UpdateHealthUi(bool animate = true)
	{
		var maximumHealth = Mathf.Clamp(GameManager.Instance.MaxHealth, 0, _heartSlots.Count);
		var currentHealth = Mathf.Clamp(GameManager.Instance.GetHealth(), 0, maximumHealth);
		var firstAvailableSlot = _heartSlots.Count - maximumHealth;
		var firstFilledSlot = _heartSlots.Count - currentHealth;

		for (var index = 0; index < _heartSlots.Count; index++)
		{
			_heartSlots[index].Visible = index >= firstAvailableSlot;
			_heartFills[index].Visible = index >= firstFilledSlot;
		}

		if (animate && firstFilledSlot < _heartFills.Count)
		{
			CreateHeartScaleTween(_heartFills[firstFilledSlot]);
		}
	}

	private void UpdateActionSlotUi(bool hasNuke, bool animate = true)
	{
		_actionSlotTween?.Kill();
		_actionIcon.PivotOffset = _actionIcon.Size / 2.0f;
		_actionIcon.Scale = Vector2.One;
		_actionSlot.SelfModulate = hasNuke
			? Colors.White
			: new Color(1.0f, 1.0f, 1.0f, 0.78f);
		_actionIcon.SelfModulate = hasNuke
			? Colors.White
			: new Color(0.55f, 0.62f, 0.7f, 0.18f);
		_actionStatusLabel.Text = hasNuke ? "READY\nSPACE" : "EMPTY";
		_actionStatusLabel.Modulate = hasNuke
			? new Color(Constants.CustomColors.BlueLightBright)
			: new Color(0.65f, 0.7f, 0.78f, 0.75f);

		if (!animate || !hasNuke)
		{
			return;
		}

		_actionSlotTween = CreateTween();
		_actionSlotTween.TweenProperty(
			_actionIcon,
			"scale",
			Vector2.One * 1.22f,
			0.14f
		).SetTrans(Tween.TransitionType.Back)
		.SetEase(Tween.EaseType.Out);
		_actionSlotTween.TweenProperty(
			_actionIcon,
			"scale",
			Vector2.One,
			0.18f
		).SetTrans(Tween.TransitionType.Back)
		.SetEase(Tween.EaseType.Out);
	}

	private void PlayNukeFlash()
	{
		_nukeFlashTween?.Kill();
		_nukeFlash.Visible = true;
		_nukeFlash.SelfModulate = new Color(1.0f, 0.92f, 0.45f, 0.88f);

		_nukeFlashTween = CreateTween();
		_nukeFlashTween.TweenProperty(
			_nukeFlash,
			PropertyName.SelfModulate.ToString(),
			Colors.Transparent,
			0.48f
		).SetTrans(Tween.TransitionType.Cubic)
		.SetEase(Tween.EaseType.Out);
		_nukeFlashTween.TweenCallback(Callable.From(() => _nukeFlash.Visible = false));
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
		_colorScaleTween?.Kill();
		_heartScaleTween?.Kill();
		_musicVolumeTween?.Kill();
		_eventLabelTween?.Kill();
		_actionSlotTween?.Kill();
		_nukeFlashTween?.Kill();
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



#region Asynchronous Tasks

	private async Task CreateTimerAsync(float timeInSeconds)
	{
		var tree = GetTree();

		if (tree == null)
		{
			return;
		}

		await ToSignal(
			tree.CreateTimer(timeInSeconds, false),
			SceneTreeTimer.SignalName.Timeout
		);
	}
	
	private async Task PlayGameStartSequenceAsync()
  {
		_audioCommencingMission.Play();

		await CreateTimerAsync(1.5f);

		if (!IsActiveInTree())
		{
			return;
		}

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
		_audioStageAdvancement.Play();

		await CreateTimerAsync(1.75f);

		if (!IsActiveInTree())
		{
			return;
		}

		PlayStageAdvancementCommanderAudio();
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

	private void TryDetonateNuke()
	{
		if (_isDying || !GameManager.Instance.TryUseNuke())
		{
			return;
		}

		var clearedTargets = ClearNukeTargets();
		_audioExplosion.Play();
		_camera.ScreenShake(_shakeIntensity + 15, _shakeTime + 0.15f);
		PlayNukeFlash();
		ShowEventMessage(
			$"NUKE DETONATED - {clearedTargets} CLEARED",
			new Color(Constants.CustomColors.YellowBright),
			1.1f
		);
	}

	private int ClearNukeTargets()
	{
		return ClearFallingObjects(Constants.GroupNames.NukeTargets);
	}

	private int ClearFallingObjects(string groupName)
	{
		var clearedTargets = 0;
		var targets = GetTree().GetNodesInGroup(groupName);

		foreach (var target in targets)
		{
			if (!GodotObject.IsInstanceValid(target)
					|| target.IsQueuedForDeletion()
					|| !IsAncestorOf(target))
			{
				continue;
			}

			if (target is GemPowerUp)
			{
				SignalManager.Instance.EmitPowerUpRemoved();
			}

			target.QueueFree();
			clearedTargets++;
		}

		return clearedTargets;
	}
	
	private void IncurDamage()
	{
		_hurtSound.Play();
		_camera.ScreenShake(_shakeIntensity, _shakeTime);
		UpdateHealthUi();
	}

	//TODO: Move to Score Manager
	private void IncrementScore(int points)
	{
		_score += points;
		SignalManager.Instance.EmitScoreIncremented(_score);
	}

	private void IncrementScoreSilently(int points)
	{
		_score += points;
	}

	private void ApplyStageClearReward()
	{
		IncrementScoreSilently(STAGE_CLEAR_BONUS_POINTS);
		UpdateScoreText();

		if (GameManager.Instance.GetHealth() < GameManager.Instance.MaxHealth)
		{
			SignalManager.Instance.EmitHealthRecovered();
		}
	}

	private void IncrementCatchStreak()
	{
		_catchStreak++;
	}

	private void ResetCatchStreak()
	{
		_catchStreak = 0;
		UpdateScoreText();
	}

	private int GetCatchScoreValue()
	{
		if (_catchStreak > 0 && _catchStreak % STREAK_BONUS_INTERVAL == 0)
		{
			return DEFAULT_POINT_VALUE + STREAK_BONUS_POINTS;
		}

		return DEFAULT_POINT_VALUE;
	}

	private string GetScoreLabelText()
	{
		if (_catchStreak >= MIN_VISIBLE_STREAK)
		{
			return $"Score: {_score:000}  Streak: {_catchStreak}";
		}

		return $"Score: {_score:000}";
	}

	private void UpdateScoreText()
	{
		_scoreLabel.Text = GetScoreLabelText();
	}

	private void ShowMeteorStageMessage()
	{
		if (GameManager.Instance.CurrentStage < METEOR_UNLOCK_STAGE)
		{
			return;
		}

		var hazardLevel = GameManager.Instance.CurrentStage - METEOR_UNLOCK_STAGE + 1;
		var message = hazardLevel == 1
			? "METEORS INBOUND"
			: $"METEOR THREAT x{hazardLevel}";

		ShowEventMessage(message, new Color(Constants.CustomColors.OrangeBright), 1.5f);
	}

	private void UpdateMeteorStormCountdown()
	{
		_meteorStormLabel.Text = $"METEOR STORM  {_meteorStormTimeRemaining:0.0}s";
	}

	private void ShowEventMessage(string message, Color color, float displaySeconds)
	{
		_eventLabelTween?.Kill();
		_eventLabel.Text = message;
		_eventLabel.Modulate = color;
		_eventLabel.Visible = true;

		var transparentColor = color;
		transparentColor.A = 0.0f;

		_eventLabelTween = CreateTween();
		_eventLabelTween.TweenInterval(displaySeconds);
		_eventLabelTween.TweenProperty(
			_eventLabel,
			PropertyName.Modulate.ToString(),
			transparentColor,
			0.35f
		).SetTrans(Tween.TransitionType.Cubic)
		.SetEase(Tween.EaseType.In);
		_eventLabelTween.TweenCallback(Callable.From(() => _eventLabel.Visible = false));
	}

	private bool IsActiveInTree()
	{
		return GodotObject.IsInstanceValid(this) && IsInsideTree();
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
