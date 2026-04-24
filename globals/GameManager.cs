using Godot;

public partial class GameManager : Node
{
	public static GameManager Instance {get; private set;}

	private int _missedGemsCount = 0;

	public int MissedGemsCount
	{
		get => _missedGemsCount;
		set
		{
			_missedGemsCount = value;
			GD.Print("Missed Gems: " + _missedGemsCount.ToString());
		} 
	}

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
		_missedGemsCount = 0;
	}
}
