using Godot;
using System;

public partial class Game : Node2D
{
	[Export] private Paddle paddle;
	[Export] private Gem _gem;

	public override void _Ready()
	{
		SubscribeToSignals();
	}

	public override void _Process(double delta)
	{
	}

	private void SubscribeToSignals()
	{
		_gem.OnScored += OnScored;
	}

	private void OnScored()
	{
		GD.Print("YOU SCORED!");
	}
}
