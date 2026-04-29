using Godot;

public partial class SignalManager : Node
{
	public static SignalManager Instance { get; private set;}

	[Signal] public delegate void GameOverEventHandler();
	[Signal] public delegate void InitiateDeathSequenceEventHandler();
	[Signal] public delegate void BoostFuelDepletedEventHandler();
	[Signal] public delegate void BoostDisengagedEventHandler();
	[Signal] public delegate void LowFuelRangeEnteredEventHandler();
	[Signal] public delegate void LowFuelRangeExitedEventHandler();

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

	public void EmitBoostDisengaged()
	{
		EmitSignal(SignalName.BoostDisengaged);
	}

	public void EmitLowFuelRangeEntered()
	{
		EmitSignal(SignalName.LowFuelRangeEntered);
	}
}
