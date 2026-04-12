using Godot;

public partial class Gem : Area2D
{
	[Export] float _movementSpeed = 100;
	private float _speedVariation;

	public override void _Ready()
	{
		_speedVariation = GetSpeedVariation();
	}

	public override void _Process(double delta)
	{
		HandleGemMovement(delta);
	}

	private void HandleGemMovement(double delta)
	{
		Position = new Vector2(Position.X, Position.Y + (_movementSpeed * _speedVariation * (float)delta));
	}

	private float GetSpeedVariation()
	{
		var zeroOffset = 1;
		return GD.Randf() + zeroOffset;
	}
}
