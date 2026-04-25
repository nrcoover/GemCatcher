using Godot;

public partial class MainMenu : Node2D
{
	[Export] PackedScene Paddle {get; set;}

	public override void _Ready()
	{
		InstantiatePaddle();
	}

		private void InstantiatePaddle()
	{
		var paddle = (MenuPaddle)Paddle.Instantiate();
		AddChild(paddle);
	}
}
