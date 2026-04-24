using Godot;

public partial class SignalManager : Node
{
	public static SignalManager Instance { get; private set;}

	[Signal] public delegate void GameOverEventHandler();

	public override void _Ready()
	{
		Instance ??= this;
	}

	public void EmitGameOverSignal()
	{
		EmitSignal(SignalName.GameOver);
	}
}
