using Godot;

public partial class ScoreManager : Node
{
	private const string SCORE_FILE_PATH = "user://gemScore.tres";
	private const int DEFAULT_SCORE = 0;

	public static ScoreManager Instance { get; private set; }

	private int _highScore = DEFAULT_SCORE;
	private int _highScoreRestore = DEFAULT_SCORE;

	public int HighScore
	{
		get
		{
			return _highScore;
		}
		set
		{
			value = Mathf.Max(0, value);

			if (value > _highScore)
			{
				_highScore = value;

				if (value > _highScoreRestore)
				{
					_highScoreRestore = value;
				}

				SaveScoreToFile();
			}
		}
	}

	public int HighScoreRestore
	{
		get
		{
			return _highScoreRestore;
		}
		set
		{
			value = Mathf.Max(0, value);

			if (value > _highScoreRestore)
			{
				_highScoreRestore = value;
				SaveScoreToFile();
			}
		}
	}

	public override void _Ready()
	{
		Instance = this;
		LoadScoreFromFile();
	}

	public void ResetHighScore()
	{
		_highScore = DEFAULT_SCORE;

		SaveScoreToFile();

		SignalManager.Instance.EmitHighScoreChangedSignal();
	}

	public void RestoreHighScore()
	{
		_highScore = _highScoreRestore;

		SaveScoreToFile();

		SignalManager.Instance.EmitHighScoreChangedSignal();
	}

	private void SaveScoreToFile()
	{
		var highScoreResource = new HighScoreResource
		{
			HighScore = _highScore,
			HighScoreRestore = _highScoreRestore
		};

		ResourceSaver.Save(highScoreResource, SCORE_FILE_PATH);
	}

	private void LoadScoreFromFile()
	{
		if (!ResourceLoader.Exists(SCORE_FILE_PATH))
		{
			return;
		}

		var highScoreResource = ResourceLoader.Load<HighScoreResource>(SCORE_FILE_PATH);

		if (highScoreResource != null)
		{
			_highScore = highScoreResource.HighScore;
			_highScoreRestore = highScoreResource.HighScoreRestore;
		}
	}
}