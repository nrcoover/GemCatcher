using Godot;

public partial class GemSpawner : Node2D
{
	const float WAIT_TIME_WINDOW_BOTTOM_CONSTRAINT = 5.0f;
	const float WAIT_TIME_WINDOW_CONSTRAINT = 3.0f;
	const float WAIT_TIME_WINDOW_TOP_CONSTRAINT = WAIT_TIME_WINDOW_BOTTOM_CONSTRAINT + WAIT_TIME_WINDOW_CONSTRAINT;
	const int METEOR_UNLOCK_STAGE = 3;
	const float METEOR_MINIMUM_WAIT_TIME = 2.25f;
	const float METEOR_BASE_MINIMUM_WAIT_TIME = 5.5f;
	const float METEOR_BASE_MAXIMUM_WAIT_TIME = 7.0f;
	const float METEOR_WAIT_TIME_REDUCTION_PER_STAGE = 0.45f;
	const int METEOR_STORM_UNLOCK_STAGE = 4;
	const float METEOR_STORM_DURATION = 14.0f;
	const float METEOR_STORM_MINIMUM_WAIT_TIME = 24.0f;
	const float METEOR_STORM_BASE_MINIMUM_WAIT_TIME = 32.0f;
	const float METEOR_STORM_BASE_MAXIMUM_WAIT_TIME = 46.0f;
	const float METEOR_STORM_WAIT_TIME_REDUCTION_PER_STAGE = 0.75f;
	const float METEOR_STORM_MINIMUM_SPAWN_WAIT_TIME = 0.75f;
	const float METEOR_STORM_MAXIMUM_SPAWN_WAIT_TIME = 1.15f;
	const int STARDUST_UNLOCK_STAGE = 2;
	const int STARDUST_MINIMUM_GEMS = 6;
	const int STARDUST_MAXIMUM_GEMS = 9;
	const float STARDUST_MINIMUM_WAIT_TIME = 13.0f;
	const float STARDUST_BASE_MINIMUM_WAIT_TIME = 18.0f;
	const float STARDUST_BASE_MAXIMUM_WAIT_TIME = 26.0f;
	const float STARDUST_WAIT_TIME_REDUCTION_PER_STAGE = 0.5f;
	const int NUKE_SPAWN_CHANCE = 28;

	[Export] private Timer _gemSpawnTimer;
	[Export] private Timer _powerUpSpawnTimer;
	[Export] private Timer _meteorSpawnTimer;
	[Export] private Timer _stardustEventTimer;
	[Export] private Timer _stardustWaveTimer;
	[Export] private Timer _meteorStormEventTimer;
	[Export] private Timer _meteorStormSpawnTimer;
	[Export] private Timer _meteorStormDurationTimer;
	[Export] private PackedScene _gemScene;
	[Export] private PackedScene _gemHeartScene;
	[Export] private PackedScene _gemPowerUpScene;
	[Export] private PackedScene _meteorScene;
	[Export] private PackedScene _bonusGemScene;
	[Export] private PackedScene _gemNukeScene;
	[Export] private Node _gemContainer;
	[Export] private bool _isOnMainMenu;
	[Export] private bool _isPrimarySpawner = true;

	public float SpawnTime = 1.75f;

	private Marker2D _leftBoundary;
	private Marker2D _rightBoundary;
	private float _originalGemSpawnWaitTime;
	private bool _canSpawnPowerUp;
	private float _difficultyTimeBalancer = 0;
	private int _remainingStardustGems;
	private bool _isStardustShowerActive;
	private bool _isMeteorStormPending;

	public override void _Ready()
	{
		InitializeVariables();

		if (_isOnMainMenu)
		{
			SetBoundaryMarkers();
		}
		else
		{
			if (GameManager.Instance.IsMeteorStormActive)
			{
				_gemSpawnTimer.Stop();
			}
			else
			{
				SpawnGem();
			}

			if (_isPrimarySpawner)
			{
				StartPowerUpTimer();
				TryStartMeteorTimer();
				TryStartStardustEventTimer();
				TryStartMeteorStormEventTimer();
			}
		}

		SubscribeToSignals();
	}

	public override void _ExitTree()
	{
		UnsubscribeFromSignals();
	}

	private void InitializeVariables()
	{
		_gemSpawnTimer.WaitTime = SpawnTime;
		_originalGemSpawnWaitTime = SpawnTime;
		_canSpawnPowerUp = false;
	}

  private void SubscribeToSignals()
	{
		_gemSpawnTimer.Timeout += OnGemSpawnTimeout;
		_powerUpSpawnTimer.Timeout += OnPowerUpSpawnTimeout;
		_meteorSpawnTimer.Timeout += OnMeteorSpawnTimeout;
		_stardustEventTimer.Timeout += OnStardustEventTimeout;
		_stardustWaveTimer.Timeout += OnStardustWaveTimeout;
		_meteorStormEventTimer.Timeout += OnMeteorStormEventTimeout;
		_meteorStormSpawnTimer.Timeout += OnMeteorStormSpawnTimeout;
		_meteorStormDurationTimer.Timeout += OnMeteorStormDurationTimeout;
		SignalManager.Instance.DifficultyIncreased += OnDifficultyIncreased;
		SignalManager.Instance.AdvanceStage += OnAdvanceStage;
		SignalManager.Instance.MeteorStormStarted += OnMeteorStormStarted;
		SignalManager.Instance.MeteorStormEnded += OnMeteorStormEnded;
	}

	private void OnGemSpawnTimeout()
	{
		if (!_isOnMainMenu && GameManager.Instance.IsMeteorStormActive)
		{
			_gemSpawnTimer.Stop();
			return;
		}

		SpawnGemType();
	}

  private void OnPowerUpSpawnTimeout()
  {
    EnablePowerUpSpawning();
		GD.Print("Wait Time ended. Can spawn power ups is: " + _canSpawnPowerUp);
  }

	private void OnMeteorSpawnTimeout()
	{
		if (!_isPrimarySpawner
				|| GameManager.Instance.CurrentStage < METEOR_UNLOCK_STAGE
				|| GameManager.Instance.IsMeteorStormActive)
		{
			return;
		}

		SpawnMeteor();
		StartMeteorTimer();
	}

	private void OnStardustEventTimeout()
	{
		if (!_isPrimarySpawner
				|| _isOnMainMenu
				|| GameManager.Instance.IsMeteorStormActive
				|| GameManager.Instance.CurrentStage < STARDUST_UNLOCK_STAGE)
		{
			return;
		}

		_isStardustShowerActive = true;
		_remainingStardustGems = Helper.GetRandomInt(
			STARDUST_MINIMUM_GEMS,
			STARDUST_MAXIMUM_GEMS
		);
		SignalManager.Instance.EmitStardustShowerStarted();
		SpawnNextStardustGem();
	}

	private void OnStardustWaveTimeout()
	{
		if (GameManager.Instance.IsMeteorStormActive)
		{
			return;
		}

		SpawnNextStardustGem();
	}

	private void OnMeteorStormEventTimeout()
	{
		if (!_isPrimarySpawner
				|| _isOnMainMenu
				|| GameManager.Instance.IsMeteorStormActive
				|| GameManager.Instance.CurrentStage < METEOR_STORM_UNLOCK_STAGE)
		{
			return;
		}

		if (_isStardustShowerActive)
		{
			_isMeteorStormPending = true;
			return;
		}

		StartMeteorStorm();
	}

	private void OnMeteorStormSpawnTimeout()
	{
		if (!GameManager.Instance.IsMeteorStormActive)
		{
			return;
		}

		SpawnMeteor();
		StartMeteorStormSpawnTimer();
	}

	private void OnMeteorStormDurationTimeout()
	{
		FinishMeteorStorm();
	}

  private void UnsubscribeFromSignals()
	{
		_gemSpawnTimer.Timeout -= OnGemSpawnTimeout;
		_powerUpSpawnTimer.Timeout -= OnPowerUpSpawnTimeout;
		_meteorSpawnTimer.Timeout -= OnMeteorSpawnTimeout;
		_stardustEventTimer.Timeout -= OnStardustEventTimeout;
		_stardustWaveTimer.Timeout -= OnStardustWaveTimeout;
		_meteorStormEventTimer.Timeout -= OnMeteorStormEventTimeout;
		_meteorStormSpawnTimer.Timeout -= OnMeteorStormSpawnTimeout;
		_meteorStormDurationTimer.Timeout -= OnMeteorStormDurationTimeout;
		SignalManager.Instance.DifficultyIncreased -= OnDifficultyIncreased;
		SignalManager.Instance.AdvanceStage -= OnAdvanceStage;
		SignalManager.Instance.MeteorStormStarted -= OnMeteorStormStarted;
		SignalManager.Instance.MeteorStormEnded -= OnMeteorStormEnded;
	}

	private void OnAdvanceStage()
	{
		TryStartMeteorTimer();
		TryStartStardustEventTimer();
		TryStartMeteorStormEventTimer();
	}

  private void OnDifficultyIncreased()
  {
		var decrementalTime = 0.01;
		var minWaitTime = 0.75f; // Clamp at .25 or .15 wait time; .75 when mixed with speed increase on difficulty level increase.

    _gemSpawnTimer.WaitTime = (float)Mathf.Clamp(
			_gemSpawnTimer.WaitTime -= decrementalTime
			, minWaitTime
			, _originalGemSpawnWaitTime
		);

		var timeBalancerIncrementor = 0.05f;
		_difficultyTimeBalancer += timeBalancerIncrementor;
		
		GD.Print($"NEW WAIT TIME: {_gemSpawnTimer.WaitTime}");
  }

	private void SetBoundaryMarkers()
  {
    _leftBoundary = (Marker2D)GetNode("../../MainMenu/CanvasLayer/MainMenuUi/MarginContainer/TitleContainer/LeftBoundary");

		_rightBoundary = (Marker2D)GetNode("../../MainMenu/CanvasLayer/MainMenuUi/MarginContainer/TitleContainer/RightBoundary");
  }

	private void SpawnGemType()
	{
		if (!_isOnMainMenu && GameManager.Instance.IsMeteorStormActive)
		{
			return;
		}

		if (_isOnMainMenu)
		{
			SpawnGem();
			return;
		}

		if (!_isPrimarySpawner)
		{
			SpawnGem();
			return;
		}

		var heartSpawnNumber = 10;
		var powerUpSpawnNumber = 8;
		var randomNumber = Helper.GetRandomInt(1, heartSpawnNumber);
		var nukeRandomNumber = Helper.GetRandomInt(1, NUKE_SPAWN_CHANCE);

		if (nukeRandomNumber >= NUKE_SPAWN_CHANCE)
		{
			SpawnNukeGem();
		}
		else if (GameManager.Instance.GetHealth() < GameManager.Instance.MaxHealth
				&& randomNumber >= heartSpawnNumber)
		{
			SpawnHeartGem();
		}
		else if (randomNumber >= powerUpSpawnNumber && _canSpawnPowerUp)
		{
			SpawnPowerUpGem();
			SignalManager.Instance.EmitPowerUpSpawned();
			StartPowerUpTimer();
		}
		else
		{
			SpawnGem();			
		}
	}

	private void SpawnGem()
	{
		var gem = (Gem)_gemScene.Instantiate();
		_gemContainer.AddChild(gem);

		if (_isOnMainMenu)
		{
			SetGemPositionOnMainMenu(gem);

			return;
		}

		SetGemSpeedByDifficultyLevel(gem);
		SetGemPosition(gem);
	}

  private void SpawnHeartGem()
	{
		if (_isOnMainMenu)
		{
			return;
		}

		var heartGem = (GemHeart)_gemHeartScene.Instantiate();
		_gemContainer.AddChild(heartGem);

		SetGemPosition(heartGem);
	}

	private void SpawnPowerUpGem()
	{
		if (_isOnMainMenu)
		{
			return;
		}

		var powerUpGem = (GemPowerUp)_gemPowerUpScene.Instantiate();
		_gemContainer.AddChild(powerUpGem);

		SetGemPosition(powerUpGem);
	}

	private void SpawnNukeGem()
	{
		var nukeGem = (GemNuke)_gemNukeScene.Instantiate();
		_gemContainer.AddChild(nukeGem);
		SetGemPosition(nukeGem, 95);
	}

	private void SpawnMeteor()
	{
		var meteor = (Meteor)_meteorScene.Instantiate();
		_gemContainer.AddChild(meteor);
		SetGemPosition(meteor, 110);
	}

	private void SpawnBonusGem()
	{
		var bonusGem = (BonusGem)_bonusGemScene.Instantiate();
		_gemContainer.AddChild(bonusGem);
		_gemContainer.MoveChild(bonusGem, 0);
		SetGemPosition(bonusGem, 75);
	}

	private void SetGemPosition(Node2D gem, int margin = 85)
	{
		var xBoundaryCoordinate = Helper.GetRandomFloat(
				GetViewportRect().Position.X + margin, 
				GetViewportRect().End.X - margin
			);

		gem.Position = new Vector2(
				xBoundaryCoordinate,
				-margin
			);
	}

	private void SetGemPositionOnMainMenu(Gem gem)
	{
		if (_leftBoundary == null || _rightBoundary == null)
		{
			SetGemPosition(gem);
			return;
		}

		var xBoundaryCoordinate = Helper.GetRandomFloat(
				_rightBoundary.GlobalPosition.X,
				_leftBoundary.GlobalPosition.X 
			);

		gem.Position = new Vector2(
				xBoundaryCoordinate,
				_leftBoundary.GlobalPosition.Y
			);
	}

  private void SetGemSpeedByDifficultyLevel(Gem gem)
  {
    gem.SpeedVariation *= GameManager.Instance.DifficultyLevel;
  }

	private void SetPowerUpTimerWaitTime()
	{
		var minWaitTime = 10.0f - _difficultyTimeBalancer;
		var maxWaitTime = 15.0f - _difficultyTimeBalancer;

		if (minWaitTime < WAIT_TIME_WINDOW_BOTTOM_CONSTRAINT)
		{
			minWaitTime = WAIT_TIME_WINDOW_BOTTOM_CONSTRAINT;
		}

		if (maxWaitTime < WAIT_TIME_WINDOW_TOP_CONSTRAINT)
		{
			maxWaitTime = WAIT_TIME_WINDOW_TOP_CONSTRAINT;
		}

		var waitTime = Helper.GetRandomFloat(minWaitTime, maxWaitTime);
		_powerUpSpawnTimer.WaitTime = waitTime;

		GD.Print("Power Up Wait Time Set: " + waitTime + " seconds. Can spawn power ups is: " + _canSpawnPowerUp);
	}

	private void StartPowerUpTimer()
	{
		DisablePowerUpSpawning();
		SetPowerUpTimerWaitTime();

		GD.Print("Power Up Timer Started! Wait time is: " + _powerUpSpawnTimer.WaitTime + " Can spawn power ups is: " + _canSpawnPowerUp);

		_powerUpSpawnTimer.Start();
	}

	private void DisablePowerUpSpawning()
	{
		_canSpawnPowerUp = false;
	}

	private void EnablePowerUpSpawning()
	{
		_canSpawnPowerUp = true;
	}

	private void TryStartMeteorTimer()
	{
		if (!_isPrimarySpawner
				|| _isOnMainMenu
				|| GameManager.Instance.CurrentStage < METEOR_UNLOCK_STAGE
				|| GameManager.Instance.IsMeteorStormActive
				|| !_meteorSpawnTimer.IsStopped())
		{
			return;
		}

		StartMeteorTimer();
	}

	private void StartMeteorTimer()
	{
		var hazardLevel = GameManager.Instance.CurrentStage - METEOR_UNLOCK_STAGE;
		var waitTimeReduction = hazardLevel * METEOR_WAIT_TIME_REDUCTION_PER_STAGE;
		var minimumWaitTime = Mathf.Max(
			METEOR_MINIMUM_WAIT_TIME,
			METEOR_BASE_MINIMUM_WAIT_TIME - waitTimeReduction
		);
		var maximumWaitTime = Mathf.Max(
			minimumWaitTime + 0.75f,
			METEOR_BASE_MAXIMUM_WAIT_TIME - waitTimeReduction
		);

		_meteorSpawnTimer.WaitTime = Helper.GetRandomFloat(minimumWaitTime, maximumWaitTime);
		_meteorSpawnTimer.Start();
	}

	public void ConfigureAsAdditionalSpawner()
	{
		_isPrimarySpawner = false;
	}

	private void TryStartStardustEventTimer()
	{
		if (!_isPrimarySpawner
				|| _isOnMainMenu
				|| _isStardustShowerActive
				|| GameManager.Instance.IsMeteorStormActive
				|| GameManager.Instance.CurrentStage < STARDUST_UNLOCK_STAGE
				|| !_stardustEventTimer.IsStopped())
		{
			return;
		}

		StartStardustEventTimer();
	}

	private void StartStardustEventTimer()
	{
		var eventLevel = GameManager.Instance.CurrentStage - STARDUST_UNLOCK_STAGE;
		var waitTimeReduction = eventLevel * STARDUST_WAIT_TIME_REDUCTION_PER_STAGE;
		var minimumWaitTime = Mathf.Max(
			STARDUST_MINIMUM_WAIT_TIME,
			STARDUST_BASE_MINIMUM_WAIT_TIME - waitTimeReduction
		);
		var maximumWaitTime = Mathf.Max(
			minimumWaitTime + 4.0f,
			STARDUST_BASE_MAXIMUM_WAIT_TIME - waitTimeReduction
		);

		_stardustEventTimer.WaitTime = Helper.GetRandomFloat(minimumWaitTime, maximumWaitTime);
		_stardustEventTimer.Start();
	}

	private void SpawnNextStardustGem()
	{
		if (_remainingStardustGems <= 0)
		{
			FinishStardustShower();
			return;
		}

		SpawnBonusGem();
		_remainingStardustGems--;

		if (_remainingStardustGems > 0)
		{
			_stardustWaveTimer.Start();
		}
		else
		{
			FinishStardustShower();
		}
	}

	private void FinishStardustShower()
	{
		_isStardustShowerActive = false;

		if (_isMeteorStormPending)
		{
			_isMeteorStormPending = false;
			StartMeteorStorm();
			return;
		}

		StartStardustEventTimer();
		TryStartMeteorStormEventTimer();
	}

	private void TryStartMeteorStormEventTimer()
	{
		if (!_isPrimarySpawner
				|| _isOnMainMenu
				|| _isStardustShowerActive
				|| GameManager.Instance.IsMeteorStormActive
				|| GameManager.Instance.CurrentStage < METEOR_STORM_UNLOCK_STAGE
				|| !_meteorStormEventTimer.IsStopped())
		{
			return;
		}

		var stormLevel = GameManager.Instance.CurrentStage - METEOR_STORM_UNLOCK_STAGE;
		var waitTimeReduction = stormLevel * METEOR_STORM_WAIT_TIME_REDUCTION_PER_STAGE;
		var minimumWaitTime = Mathf.Max(
			METEOR_STORM_MINIMUM_WAIT_TIME,
			METEOR_STORM_BASE_MINIMUM_WAIT_TIME - waitTimeReduction
		);
		var maximumWaitTime = Mathf.Max(
			minimumWaitTime + 8.0f,
			METEOR_STORM_BASE_MAXIMUM_WAIT_TIME - waitTimeReduction
		);

		_meteorStormEventTimer.WaitTime = Helper.GetRandomFloat(minimumWaitTime, maximumWaitTime);
		_meteorStormEventTimer.Start();
	}

	private void StartMeteorStorm()
	{
		_isMeteorStormPending = false;
		GameManager.Instance.BeginMeteorStorm();
		_meteorSpawnTimer.Stop();
		_stardustEventTimer.Stop();
		SignalManager.Instance.EmitMeteorStormStarted(METEOR_STORM_DURATION);

		SpawnMeteor();
		StartMeteorStormSpawnTimer();
		_meteorStormDurationTimer.Start(METEOR_STORM_DURATION);
	}

	private void StartMeteorStormSpawnTimer()
	{
		_meteorStormSpawnTimer.Start(Helper.GetRandomFloat(
			METEOR_STORM_MINIMUM_SPAWN_WAIT_TIME,
			METEOR_STORM_MAXIMUM_SPAWN_WAIT_TIME
		));
	}

	private void FinishMeteorStorm()
	{
		if (!GameManager.Instance.IsMeteorStormActive)
		{
			return;
		}

		_meteorStormSpawnTimer.Stop();
		GameManager.Instance.EndMeteorStorm();
		SignalManager.Instance.EmitMeteorStormEnded();
		TryStartMeteorTimer();
		TryStartStardustEventTimer();
		TryStartMeteorStormEventTimer();
	}

	private void OnMeteorStormStarted(float _duration)
	{
		if (!_isOnMainMenu)
		{
			_gemSpawnTimer.Stop();
		}
	}

	private void OnMeteorStormEnded()
	{
		if (!_isOnMainMenu && _gemSpawnTimer.IsStopped())
		{
			_gemSpawnTimer.Start();
		}
	}
}
