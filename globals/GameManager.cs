using Godot;

public partial class GameManager : Node
{
	const int MAX_HEALTH = 5;
	const int MIN_HEALTH = 0;

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
	}

	private void UnsubscribeFromSignals()
	{
		SignalManager.Instance.GameOver -= OnGameOver;
		SignalManager.Instance.HealthRecovered -= OnHealthRecovered;
	}

  public void OnGameOver()
	{
		SignalManager.Instance.EmitInitiateDeathSequence();
	}
	
  private void OnHealthRecovered()
  {
    IncrementHealth();
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

		if (GetHealth() <= MIN_HEALTH)
		{
			SignalManager.Instance.EmitGameOver();
		}
	}

#endregion
}
