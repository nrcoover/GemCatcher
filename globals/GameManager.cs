using Godot;

public partial class GameManager : Node
{
	const int MAX_MISSED_GEMS = 3;
	const int MAX_HEALTH = 3;
	const int MIN_HEALTH = 0;
	private int _high_score = 0;

	public static GameManager Instance {get; private set;}

	private int _missedGemsCount = 0;
	private int _health = MAX_HEALTH;

	public override void _Ready()
	{
		Instance ??= this;
    SubscribeToSignals();
	}

	public override void _ExitTree()
	{
		UnsubscribeFromSignals();
	}

	public void ResetGame()
	{
		SetMissedGemCount(0);
		SetHealth(MAX_HEALTH);
	}

#region Signals

	public void OnGameOver()
	{
		SignalManager.Instance.EmitInitiateDeathSequence();
	}

#endregion

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

		if (GetMissedGemCount() >= MAX_MISSED_GEMS
				&& GetHealth() <= MIN_HEALTH)
		{
			SignalManager.Instance.EmitGameOver();
		}
	}

#endregion

	private void SubscribeToSignals()
	{
		SignalManager.Instance.GameOver += OnGameOver;
	}

	private void UnsubscribeFromSignals()
	{
		SignalManager.Instance.GameOver -= OnGameOver;
	}
}
