using Godot;

public partial class SaveDataManager : Node
{
	private const string SAVE_DATA_FILE_PATH = "user://gemSaveData.tres";
	private const int DEFAULT_SCORE = 0;

	public static SaveDataManager Instance { get; private set; }

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

				SaveDataToFile();
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
				SaveDataToFile();
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
				SaveDataToFile();
			}
		}
	}

	public override void _Ready()
	{
		Instance = this;
		LoadDataFromFile();
	}

	public void ResetSaveData()
	{
		_highScore = DEFAULT_SCORE;
		_isInfiniteModeUnlocked = false;

		SaveDataToFile();

		SignalManager.Instance.EmitSaveDataResetSignal();
	}

	public void RestoreHighScore()
	{
		_highScore = _highScoreRestore;

		SaveDataToFile();

		SignalManager.Instance.EmitSaveDataResetSignal();
	}

	private void SaveDataToFile()
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

		ResourceSaver.Save(containerResource, SAVE_DATA_FILE_PATH);
	}

	private void LoadDataFromFile()
	{
		if (!ResourceLoader.Exists(SAVE_DATA_FILE_PATH))
		{
			return;
		}

		var saveData = ResourceLoader.Load<SaveGameContainerResource>(SAVE_DATA_FILE_PATH);

		if (saveData == null)
		{
			return;
		}

		var highScoreResource = saveData.HighScore;
		var isInfiniteModeUnlockedResource = saveData.InfiniteMode;

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
