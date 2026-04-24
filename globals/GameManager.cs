using Godot;

public partial class GameManager : Node
{
	const int MAX_MISSED_GEMS = 3;

	public static GameManager Instance {get; private set;}

	private int _missedGemsCount = 0;

	public int MissedGemsCount = 0;
	
	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public override void _Process(double delta)
	{
	}

	private void ResetGame()
	{
		SetMissedGemCount(0);
	}

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
		if(MissedGemsCount == MAX_MISSED_GEMS)
		{
			GD.Print("Maximum Missed Gems Exceeded! GAME OVER!");
		}
	}

#endregion
}
