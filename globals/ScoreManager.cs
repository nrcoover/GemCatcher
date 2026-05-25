using Godot;

public partial class ScoreManager : Node
{
	private const string SCORE_FILE_PATH = "user://gemScore.tres";
	private const int DEFAULT_SCORE = 0;

	public static ScoreManager Instance { get; private set;}

	private int _highScore = DEFAULT_SCORE;
	private int _highScoreRestore;

	public int HighScore
	{
		get
		{
			return _highScore;
		}
		set
		{
			_highScore = Mathf.Max(0, value);
			SaveScoreToFile();
		}
	}

	public int HighScoreRestore
	{
		get
		{
			return _highScoreRestore;
		}
	}

  public override void _Ready()
  {
    Instance = this;
		LoadScoreFromFile();
		LoadRestoreHighScoreFromFile();
  }

	public void ResetHighScore()
	{
		HighScore = DEFAULT_SCORE;
		SignalManager.Instance.EmitHighScoreChangedSignal();
	}

	public void RestoreHighScore()
	{
		HighScore = LoadRestoreHighScoreFromFile();
		SignalManager.Instance.EmitHighScoreChangedSignal();
	}

  private void SaveScoreToFile()
	{
    var highScoreResource = new HighScoreResource
    {
      HighScore = _highScore
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

	private int LoadRestoreHighScoreFromFile()
  {
    if (!ResourceLoader.Exists(SCORE_FILE_PATH))
		{
			return DEFAULT_SCORE;
		}

		var highScoreResource = ResourceLoader.Load<HighScoreResource>(SCORE_FILE_PATH);

		if (highScoreResource != null)
		{
			return highScoreResource.HighScoreRestore;
		}

		return DEFAULT_SCORE;
  }
}
