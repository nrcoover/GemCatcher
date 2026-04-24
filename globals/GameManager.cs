using Godot;

public partial class GameManager : Node
{
	const int MAX_MISSED_GEMS = 3;

	public static GameManager Instance {get; private set;}

	private int _missedGemsCount = 0;

	public override void _Ready()
	{
		Instance ??= this;
    ConnectSignals();
	}

#region Signals

	public void OnGameOver()
	{
		ResetGame();
	}

#endregion

#region Manage Gems

	public int GetMissedGemCount()
	{
		return _missedGemsCount; 
	}

	public void SetMissedGemCount(int value)
	{
		_missedGemsCount = Mathf.Abs(value);
	}

	public void IncrementMissedGems()
	{
		_missedGemsCount ++;
		GD.Print("Missed Gems: " + _missedGemsCount.ToString());

		// TODO: Replace handling Game Over with health count
		// if health count == 0, then the game is over
		// This current method doesn't work if health can be restored.
		if (_missedGemsCount >= MAX_MISSED_GEMS)
		{
			GD.Print("Maximum Missed Gems Exceeded! GAME OVER!");
			SignalManager.Instance.EmitGameOverSignal();
		}
	}

#endregion

	private void ConnectSignals()
	{
		SignalManager.Instance.GameOver += OnGameOver;
	}

	private void ResetGame()
	{
		SetMissedGemCount(0);
		GD.Print("Game Reset!");
		GetTree().ReloadCurrentScene();
	}
}
