using Godot;

public partial class GameManager : Node
{
	const int MAX_HEALTH = 5;
	const int MIN_HEALTH = 0;

	const float DEFAULT_DIFFICULTY_LEVEL = 1;
	const float DIFFICULTY_MULTIPLIER = 1.01f;

	private float _difficultyLevel = DEFAULT_DIFFICULTY_LEVEL;

	public float DifficultyLevel
	{
		get
		{
			return _difficultyLevel;
		}
		private set
		{
			_difficultyLevel = value;
		}
	}

	public int CurrentStage
	{
		get
		{
			return _currentStage;
		}
		private set
		{
			_currentStage = value;
		}
	}

	public static GameManager Instance {get; private set;}

	public int MaxHealth {
		get
		{
			return _maxHealth;
		}
		private set
		{
			_maxHealth = MAX_HEALTH;	
		}
	}

	private int _highScore = 0;
	private int _maxHealth = MAX_HEALTH;
	private int _missedGemsCount = 0;
	private int _health = MAX_HEALTH;
	private int _currentStage = 1;

	public override void _Ready()
	{
		Instance ??= this;
    SubscribeToSignals();
	}

	public override void _Process(double _delta)
	{
		CheckForGameOver();
	}

  public override void _ExitTree()
	{
		UnsubscribeFromSignals();
	}
	
#region Signals

	private void SubscribeToSignals()
	{
		SignalManager.Instance.GameOver += OnGameOver;
		SignalManager.Instance.HealthRecovered += OnHealthRecovered;
		SignalManager.Instance.ScoreIncremented += OnScoreIncremented;
	}

  private void UnsubscribeFromSignals()
	{
		SignalManager.Instance.GameOver -= OnGameOver;
		SignalManager.Instance.HealthRecovered -= OnHealthRecovered;
		SignalManager.Instance.ScoreIncremented -= OnScoreIncremented;
	}

  public void OnGameOver()
	{
		SignalManager.Instance.EmitInitiateDeathSequence();
	}
	
  private void OnHealthRecovered()
  {
    IncrementHealth();
  }
	
  private void OnScoreIncremented(int score)
  {
    HandleDifficultyLevel(score);
		HandleStageAdvancement(score);
  }

#endregion
	
  private void CheckForGameOver()
  {
    if (GetHealth() <= 0)
		{
			SignalManager.Instance.EmitGameOver();
		}
  }

	public void ResetGame()
	{
		SetMissedGemCount(0);
		SetHealth(MAX_HEALTH);
		DifficultyLevel = DEFAULT_DIFFICULTY_LEVEL;
	}

#region Manage Health

	public int GetHealth()
	{
		return _health;
	}

	private void SetHealth(int value)
	{
		if (value > MAX_HEALTH)
		{
			_health = MAX_HEALTH;
		} 
		else if (value < MIN_HEALTH) 
		{
			_health = MIN_HEALTH;
		} 
		else
		{
			_health = value;
		}
	}

	public void DecrementHealth()
	{
		SetHealth(GetHealth() - 1);
	}

	public void IncrementHealth()
	{
		SetHealth(GetHealth() + 1);
	}

#endregion

#region Manage Gems

	public int GetMissedGemCount()
	{
		return _missedGemsCount; 
	}

	private void SetMissedGemCount(int value)
	{
		_missedGemsCount = Mathf.Abs(value);
	}

	public void IncrementMissedGems()
	{
		SetMissedGemCount(GetMissedGemCount() + 1);
    DecrementHealth();
	}

#endregion

#region Manage Difficulty Level

	private void HandleDifficultyLevel(int currentScore)
	{
		var difficultyIncrementer = 10;
		var isScoreDivisibleByTen = currentScore % difficultyIncrementer == 0;

		if (!isScoreDivisibleByTen)
		{
			return;
		}

		IncreaseDifficulty(currentScore);
	}

	private void IncreaseDifficulty(int currentScore)
	{
		var maxDifficulty = 2.75; // Clamp at 2.5 or 2.75 difficulty
		GD.Print("---------------NEW LOG--------------");
		GD.Print("INCREASE DIFFICUTLY COMMENSING!!!");
		GD.Print($"Current Level: {DifficultyLevel}");
		GD.Print($"Current Score: {currentScore}");
		
		DifficultyLevel = (float)Mathf.Clamp(
			DifficultyLevel * DIFFICULTY_MULTIPLIER
			, DEFAULT_DIFFICULTY_LEVEL
			, maxDifficulty
		);
		GD.Print($"Updated Level: {DifficultyLevel}");
		GD.Print($"DIFFICULTY LEVEL CHANGE COMPLETE!!!");
		GD.Print("------------------------------------");

		SignalManager.Instance.EmitDifficultyIncreased();
	}

	private void HandleStageAdvancement(int score)
	{
		var advancementIncrementer = 10;

		if (score % advancementIncrementer == 0)
		{
			IncrementStage();
			SignalManager.Instance.EmitAdvanceStage();
		}
	}

	private void IncrementStage()
	{
		CurrentStage ++;
	}

#endregion
}
