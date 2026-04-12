using Godot;

public partial class Gem : Area2D
{
	[Export] float _movementSpeed = 100;
	[Export] float _minSpeedVariation = 1.0f;
	[Export] float _maxSpeedVariation = 3.5f;
	[Export] float _rotationSpeed = 15.0f;
	[Export] float _minRotationVariation = -0.5f;
	[Export] float _maxRotationVariation = 0.5f;
	
	private float _speedVariation;
	private float _rotationVariation;

	public override void _Ready()
	{
		_speedVariation = Helper.GetRandomFloat(_minSpeedVariation, _maxSpeedVariation);

		_rotationVariation = Helper.GetRandomFloat(_minRotationVariation, _maxRotationVariation);
	}

	public override void _Process(double delta)
	{
		HandleMovement((float)delta);
		HandleRotation((float)delta);
	}

	private void HandleMovement(float delta)
	{
		Position = new Vector2(Position.X, Position.Y + (_movementSpeed * _speedVariation * (float)delta));
	}

	private void HandleRotation(float delta)
	{
		Rotation += _rotationSpeed * _rotationVariation * delta;
	}
}
