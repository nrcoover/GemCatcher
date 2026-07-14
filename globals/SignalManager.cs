using Godot;

public partial class SignalManager : Node
{
	public static SignalManager Instance { get; private set;}

	[Signal] public delegate void GameOverEventHandler();
	[Signal] public delegate void InitiateDeathSequenceEventHandler();
	[Signal] public delegate void BoostFuelDepletedEventHandler();
	[Signal] public delegate void BoostEngagedEventHandler();
	[Signal] public delegate void BoostDisengagedEventHandler();
	[Signal] public delegate void ScoredEventHandler(Color color);
	[Signal] public delegate void GemOffScreenEventHandler();
	[Signal] public delegate void PlayerHurtEventHandler();
	[Signal] public delegate void ShowGameOverScreenEventHandler();
	[Signal] public delegate void ShowMissionFailurePanelEventHandler();
	[Signal] public delegate void ShowGameOverButtonsEventHandler();
	[Signal] public delegate void HighScoreChangedEventHandler();
	[Signal] public delegate void HealthRecoveredEventHandler();
	[Signal] public delegate void ScoreIncrementedEventHandler(int score);
	[Signal] public delegate void DifficultyIncreasedEventHandler();
	[Signal] public delegate void PowerUpCollectedEventHandler(Color color);
	[Signal] public delegate void PowerUpSpawnedEventHandler();
	[Signal] public delegate void PowerUpRemovedEventHandler();
	[Signal] public delegate void AdvanceStageEventHandler();
	[Signal] public delegate void MeteorHitEventHandler();
	[Signal] public delegate void MeteorDodgedEventHandler();
	[Signal] public delegate void StardustShowerStartedEventHandler();
	[Signal] public delegate void StardustCollectedEventHandler(Color color);
	[Signal] public delegate void NukeSlotChangedEventHandler(bool hasNuke);
	[Signal] public delegate void MeteorStormStartedEventHandler(float duration);
	[Signal] public delegate void MeteorStormEndedEventHandler();

	public override void _Ready()
	{
		Instance ??= this;
	}

	public void EmitGameOver()
	{
		EmitSignal(SignalName.GameOver);
	}

	public void EmitInitiateDeathSequence() 
	{
		EmitSignal(SignalName.InitiateDeathSequence);
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

	public void EmitHighScoreChangedSignal()
	{
		EmitSignal(SignalName.HighScoreChanged);
	}

	public void EmitHealthRecovered()
	{
		EmitSignal(SignalName.HealthRecovered);
	}

	public void EmitScoreIncremented(int score)
	{
		EmitSignal(SignalName.ScoreIncremented, score);
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

	public void EmitMeteorHit()
	{
		EmitSignal(SignalName.MeteorHit);
	}

	public void EmitMeteorDodged()
	{
		EmitSignal(SignalName.MeteorDodged);
	}

	public void EmitStardustShowerStarted()
	{
		EmitSignal(SignalName.StardustShowerStarted);
	}

	public void EmitStardustCollected(Color color)
	{
		EmitSignal(SignalName.StardustCollected, color);
	}

	public void EmitNukeSlotChanged(bool hasNuke)
	{
		EmitSignal(SignalName.NukeSlotChanged, hasNuke);
	}

	public void EmitMeteorStormStarted(float duration)
	{
		EmitSignal(SignalName.MeteorStormStarted, duration);
	}

	public void EmitMeteorStormEnded()
	{
		EmitSignal(SignalName.MeteorStormEnded);
	}
}
