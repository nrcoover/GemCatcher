using Godot;

[GlobalClass]
public partial class HighScoreResource : Resource
{
	private int _highScore;
	private int _highScoreRestore;

	[Export]
	public int HighScore
	{
		get {
			return _highScore;
		}
		set
		{
			_highScore = Mathf.Max(0, value);

			if (_highScore > _highScoreRestore)
			{
				_highScoreRestore = _highScore;
			}
		}
	}

	[Export]
	public int HighScoreRestore
	{
		get {
			return _highScoreRestore;
		}
		set
		{
			_highScoreRestore = Mathf.Max(0, value);
		}
	}
}