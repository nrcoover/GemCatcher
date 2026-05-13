using Godot;

public partial class MainMenu : Node2D
{
	[Export] PackedScene Paddle { get; set; }
	[Export] float _paddleScale = 0.75f;

	public override void _Ready()
	{
		InstantiatePaddle();
	}

	private void InstantiatePaddle()
	{
		var paddle = (MenuPaddle)Paddle.Instantiate();
		AddChild(paddle);
		paddle.Scale = new Vector2(_paddleScale, _paddleScale);
	}
}
