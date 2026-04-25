using Godot;
using System;

public partial class MainMenu : Node2D
{
	[Export] PackedScene Paddle {get; set;}

	public override void _Ready()
	{
		InstantiatePaddle();
	}

	public override void _Process(double delta)
	{
	}

		private void InstantiatePaddle()
	{
		var paddle = (MenuPaddle)Paddle.Instantiate();
		AddChild(paddle);
	}
}
