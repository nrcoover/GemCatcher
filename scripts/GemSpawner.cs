using Godot;

public partial class GemSpawner : Node2D
{
	const float WAIT_TIME_WINDOW_BOTTOM_CONSTRAINT = 5.0f;
	const float WAIT_TIME_WINDOW_CONSTRAINT = 3.0f;
	const float WAIT_TIME_WINDOW_TOP_CONSTRAINT = WAIT_TIME_WINDOW_BOTTOM_CONSTRAINT + WAIT_TIME_WINDOW_CONSTRAINT;

	[Export] private Timer _gemSpawnTimer;
	[Export] private Timer _powerUpSpawnTimer;
	[Export] private PackedScene _gemScene;
	[Export] private PackedScene _gemHeartScene;
	[Export] private PackedScene _gemPowerUpScene;
	[Export] private Node _gemContainer;
	[Export] private bool _isOnMainMenu;

	public float SpawnTime = 1.75f;

	private Marker2D _leftBoundary;
	private Marker2D _rightBoundary;
	private float _originalGemSpawnWaitTime;
	private bool _canSpawnPowerUp;
	private float _difficultyTimeBalancer = 0;

	public override void _Ready()
	{
		InitializeVariables();
		StartPowerUpTimer();

		if (_isOnMainMenu)
		{
			SetBoundaryMarkers();
		}
		else
		{
			SpawnGem();
		}

		SubscribeToSignals();
	}

	public override void _ExitTree()
	{
		UnsubscribeFromSignals();
	}

	private void InitializeVariables()
	{
		_originalGemSpawnWaitTime = (float)_gemSpawnTimer.WaitTime;
		_canSpawnPowerUp = false;
		_gemSpawnTimer.WaitTime = SpawnTime;
	}

  private void SubscribeToSignals()
	{
		_gemSpawnTimer.Timeout += OnGemSpawnTimeout;
		_powerUpSpawnTimer.Timeout += OnPowerUpSpawnTimeout;
		SignalManager.Instance.DifficultyIncreased += OnDifficultyIncreased;
	}

	private void OnGemSpawnTimeout()
	{
		SpawnGemType();
	}

  private void OnPowerUpSpawnTimeout()
  {
    EnablePowerUpSpawning();
		GD.Print("Wait Time ended. Can spawn power ups is: " + _canSpawnPowerUp);
  }

  private void UnsubscribeFromSignals()
	{
		SignalManager.Instance.DifficultyIncreased -= OnDifficultyIncreased;
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
		if (_isOnMainMenu)
		{
			SpawnGem();
			return;
		}

		var heartSpawnNumber = 10;
		var powerUpSpawnNumber = 8;
		var randomNumber = Helper.GetRandomInt(1, heartSpawnNumber);

		if (GameManager.Instance.GetHealth() < GameManager.Instance.MaxHealth
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

	private void SetGemPosition(Node2D gem)
	{
		var margin = 85;

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
}
