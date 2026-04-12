using Godot;

public partial class Gem : Area2D
{
	[Export] float _movementSpeed = 100;
	[Export] float _minSpeedVariation = 1.0f;
	[Export] float _maxSpeedVariation = 3.5f;
	
	private float _speedVariation;

	public override void _Ready()
	{
		_speedVariation = Helper.GetRandomFloat(_minSpeedVariation, _maxSpeedVariation);
	}

	public override void _Process(double delta)
	{
		HandleGemMovement(delta);
	}

	private void HandleGemMovement(double delta)
	{
		Position = new Vector2(Position.X, Position.Y + (_movementSpeed * _speedVariation * (float)delta));
	}
}
