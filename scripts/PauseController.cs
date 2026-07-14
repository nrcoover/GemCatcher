using Godot;

public partial class PauseController : Node
{
	[Export] private Control _pauseOverlay;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		SetPaused(false);
	}

	public override void _Input(InputEvent @event)
	{
		if (!@event.IsActionPressed("pause"))
		{
			return;
		}

		TogglePause();
		GetViewport().SetInputAsHandled();
	}

	public override void _ExitTree()
	{
		if (GetTree() != null)
		{
			GetTree().Paused = false;
		}
	}

	public void TogglePause()
	{
		SetPaused(!GetTree().Paused);
	}

	private void SetPaused(bool isPaused)
	{
		_pauseOverlay.Visible = isPaused;
		SetAudioPaused(GetTree().Root, isPaused);
		GetTree().Paused = isPaused;
	}

	private static void SetAudioPaused(Node node, bool isPaused)
	{
		switch (node)
		{
			case AudioStreamPlayer audioPlayer:
				audioPlayer.StreamPaused = isPaused;
				break;
			case AudioStreamPlayer2D audioPlayer2D:
				audioPlayer2D.StreamPaused = isPaused;
				break;
			case AudioStreamPlayer3D audioPlayer3D:
				audioPlayer3D.StreamPaused = isPaused;
				break;
		}

		foreach (var child in node.GetChildren())
		{
			SetAudioPaused(child, isPaused);
		}
	}
}
