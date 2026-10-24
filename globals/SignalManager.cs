using Godot;

public partial class SignalManager : Node
{
	public static SignalManager Instance { get; private set;}

	[Signal] public delegate void GameOverEventHandler();
	[Signal] public delegate void GameVictoryEventHandler();
	[Signal] public delegate void InitiateDeathSequenceEventHandler();
	[Signal] public delegate void InitiateVictorySequenceEventHandler();
	[Signal] public delegate void BoostFuelDepletedEventHandler();
	[Signal] public delegate void BoostEngagedEventHandler();
	[Signal] public delegate void BoostDisengagedEventHandler();
	[Signal] public delegate void ScoredEventHandler(Color color);
	[Signal] public delegate void GemOffScreenEventHandler();
	[Signal] public delegate void PlayerHurtEventHandler();
	[Signal] public delegate void ShowGameOverScreenEventHandler();
	[Signal] public delegate void ShowMissionFailurePanelEventHandler();
	[Signal] public delegate void ShowGameOverButtonsEventHandler();
	[Signal] public delegate void SaveDataResetEventHandler();
	[Signal] public delegate void HealthRecoveredEventHandler();
	[Signal] public delegate void ScoreIncrementedEventHandler(int score, Color color);
	[Signal] public delegate void ScoreUpdatedEventHandler(Color color);
	[Signal] public delegate void DifficultyIncreasedEventHandler();
	[Signal] public delegate void PowerUpCollectedEventHandler(Color color);
	[Signal] public delegate void PowerUpSpawnedEventHandler();
	[Signal] public delegate void PowerUpRemovedEventHandler();
	[Signal] public delegate void AdvanceStageEventHandler();
	[Signal] public delegate void ResetGameEventHandler();
	[Signal] public delegate void SpawnPoolObjectEventHandler(Vector2 position, PackedScene scene, string colorHex);

	public override void _Ready()
	{
		Instance ??= this;
	}

	public void EmitGameOver()
	{
		EmitSignal(SignalName.GameOver);
	}

	public void EmitGameVictory()
	{
		EmitSignal(SignalName.GameVictory);
	}

	public void EmitInitiateDeathSequence() 
	{
		EmitSignal(SignalName.InitiateDeathSequence);
	}

	public void EmitInitiateVictorySequence() 
	{
		EmitSignal(SignalName.InitiateVictorySequence);
	}

	public void EmitBoostFuelDepleted()
	{
		EmitSignal(SignalName.BoostFuelDepleted);
	}

	public void EmitBoostEngaged()
	{
		EmitSignal(SignalName.BoostEngaged);
	}

	public void EmitBoostDisengaged()
	{
		EmitSignal(SignalName.BoostDisengaged);
	}

	public void EmitScored(Color color)
	{
		EmitSignal(SignalName.Scored, color);
	}

	public void EmitGemOffScreen()
	{
		EmitSignal(SignalName.GemOffScreen);
	}

	public void EmitPlayerHurt()
	{
		EmitSignal(SignalName.PlayerHurt);
	}

	public void EmitShowGameOverScreen()
	{
		EmitSignal(SignalName.ShowGameOverScreen);
	}

	public void EmitShowMissionFailurePanel()
	{
		EmitSignal(SignalName.ShowMissionFailurePanel);
	}

	public void EmitShowGameOverButtons()
	{
		EmitSignal(SignalName.ShowGameOverButtons);
	}

	public void EmitSaveDataResetSignal()
	{
		EmitSignal(SignalName.SaveDataReset);
	}

	public void EmitHealthRecovered()
	{
		EmitSignal(SignalName.HealthRecovered);
	}

	public void EmitScoreIncremented(int score, Color color)
	{
		EmitSignal(SignalName.ScoreIncremented, score, color);
	}

	public void EmitScoreUpdated(Color color)
	{
		EmitSignal(SignalName.ScoreUpdated, color);
	}

	public void EmitDifficultyIncreased()
	{
		EmitSignal(SignalName.DifficultyIncreased);
	}

	public void EmitPowerUpCollected(Color color)
	{
		EmitSignal(SignalName.PowerUpCollected, color);
	}

	public void EmitPowerUpSpawned()
	{
		EmitSignal(SignalName.PowerUpSpawned);
	}

	public void EmitPowerUpRemoved()
	{
		EmitSignal(SignalName.PowerUpRemoved);
	}

	public void EmitAdvanceStage()
	{
		EmitSignal(SignalName.AdvanceStage);
	}

	public void EmitResetGame()
	{
		EmitSignal(SignalName.ResetGame);
	}

	public static void EmitSpawnPoolObject(Vector2 position, PackedScene scene, string colorHex = null)
	{
		Instance.EmitSignal(SignalName.SpawnPoolObject, position, scene, colorHex);
	}
}
