using Godot;

public partial class FalseGem : Gem
{
	public override void _Ready()
	{
		base._Ready();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}

#region Signals

	public override void OnAreaEntered(Area2D area)
	{
		if (area is Paddle || area is PaddleGhost)
		{
			Deactivate();
		} else if (area is MenuPaddle)
		{
			Deactivate();
		}
	}

#endregion

	protected override void HandleExitScreen()
	{
		if (Position.Y > GetViewportRect().End.Y)
		{
			_isOffScreen = true;
		}
	}
}
