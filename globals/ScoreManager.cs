using Godot;

public partial class ScoreManager : Node
{
	private const string SCORE_FILE_PATH = "user://gemSaveData.tres";
	private const int DEFAULT_SCORE = 0;

	public static ScoreManager Instance { get; private set; }

	private int _highScore = DEFAULT_SCORE;
	private int _highScoreRestore = DEFAULT_SCORE;

	private bool _isInfiniteModeUnlocked = false;

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

	public bool IsInfiniteModeUnlocked
	{
		get
		{
			return _isInfiniteModeUnlocked;
		}
		set
		{
			{
				_isInfiniteModeUnlocked = value;
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
		_isInfiniteModeUnlocked = false;

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

		var isInfiniteModeUnlockedResource = new InfiniteModeResource
		{
			IsInfiniteModeUnlocked = _isInfiniteModeUnlocked
		};

		var containerResource = new SaveGameContainerResource
		{
			InfiniteMode = isInfiniteModeUnlockedResource,
			HighScore = highScoreResource
		};

		ResourceSaver.Save(containerResource, SCORE_FILE_PATH);
	}

	private void LoadScoreFromFile()
	{
		if (!ResourceLoader.Exists(SCORE_FILE_PATH))
		{
			return;
		}

		var saveData = ResourceLoader.Load<SaveGameContainerResource>(SCORE_FILE_PATH);

		if (saveData == null)
		{
			return;
		}

		var highScoreResource = new HighScoreResource();
		var isInfiniteModeUnlockedResource = new InfiniteModeResource();

		highScoreResource = saveData.HighScore;
		isInfiniteModeUnlockedResource = saveData.InfiniteMode;

		if (highScoreResource != null)
		{
			_highScore = highScoreResource.HighScore;
			_highScoreRestore = highScoreResource.HighScoreRestore;
		}

		if (isInfiniteModeUnlockedResource != null)
		{
			_isInfiniteModeUnlocked = isInfiniteModeUnlockedResource.IsInfiniteModeUnlocked;
		}
	}
}
