using Godot;

public partial class GameManager : Node
{
	const int DEFAULT_MAX_HEALTH = 5;
	const int KID_MODE_MAX_HEALTH = DEFAULT_MAX_HEALTH * 2;
	const int MIN_HEALTH = 0;

	const float DEFAULT_DIFFICULTY_LEVEL = 1;
	const float DIFFICULTY_MULTIPLIER = 1.01f;
	const int DEFAULT_STAGE = 1;
	const int DIFFICULTY_SCORE_INCREMENT = 10;
	const int STAGE_SCORE_INCREMENT = 10;
	const float KID_MODE_FRIENDLY_SCALE_MULTIPLIER = 2.0f;
	const float KID_MODE_HAZARD_SCALE_MULTIPLIER = 0.5f;

	private float _difficultyLevel = DEFAULT_DIFFICULTY_LEVEL;
	private bool _isGameOver = false;

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
	public bool IsKidMode { get; private set; }
	public float FriendlyScaleMultiplier => IsKidMode
		? KID_MODE_FRIENDLY_SCALE_MULTIPLIER
		: 1.0f;
	public float HazardScaleMultiplier => IsKidMode
		? KID_MODE_HAZARD_SCALE_MULTIPLIER
		: 1.0f;

	public int MaxHealth => IsKidMode ? KID_MODE_MAX_HEALTH : DEFAULT_MAX_HEALTH;

	private int _highScore = 0;
	private int _missedGemsCount = 0;
	private int _health = DEFAULT_MAX_HEALTH;
	private int _currentStage = DEFAULT_STAGE;
	private int _nextDifficultyScore = DIFFICULTY_SCORE_INCREMENT;
	private int _nextStageScore = STAGE_SCORE_INCREMENT;
	private bool _hasNuke;

	public bool HasNuke => _hasNuke;
	public bool IsMeteorStormActive { get; private set; }

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
    if (!_isGameOver && GetHealth() <= 0)
		{
			_isGameOver = true;
			SignalManager.Instance.EmitGameOver();
		}
  }

	public void ResetGame()
	{
		SetMissedGemCount(0);
		SetHealth(MaxHealth);
		DifficultyLevel = DEFAULT_DIFFICULTY_LEVEL;
		_isGameOver = false;
		CurrentStage = DEFAULT_STAGE;
		_nextDifficultyScore = DIFFICULTY_SCORE_INCREMENT;
		_nextStageScore = STAGE_SCORE_INCREMENT;
		_hasNuke = false;
		IsMeteorStormActive = false;
	}

	public void SetKidMode(bool isEnabled)
	{
		IsKidMode = isEnabled;
	}

	public bool TryStoreNuke()
	{
		if (_hasNuke)
		{
			return false;
		}

		_hasNuke = true;
		SignalManager.Instance.EmitNukeSlotChanged(true);
		return true;
	}

	public bool TryUseNuke()
	{
		if (!_hasNuke)
		{
			return false;
		}

		_hasNuke = false;
		SignalManager.Instance.EmitNukeSlotChanged(false);
		return true;
	}

	public void BeginMeteorStorm()
	{
		IsMeteorStormActive = true;
	}

	public void EndMeteorStorm()
	{
		IsMeteorStormActive = false;
	}

#region Manage Health

	public int GetHealth()
	{
		return _health;
	}

	private void SetHealth(int value)
	{
		if (value > MaxHealth)
		{
			_health = MaxHealth;
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
		while (currentScore >= _nextDifficultyScore)
		{
			IncreaseDifficulty(currentScore);
			_nextDifficultyScore += DIFFICULTY_SCORE_INCREMENT;
		}
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
		while (score >= _nextStageScore)
		{
			IncrementStage();
			SignalManager.Instance.EmitAdvanceStage();
			_nextStageScore += STAGE_SCORE_INCREMENT;
		}
	}

	private void IncrementStage()
	{
		CurrentStage++;
	}

#endregion
}
