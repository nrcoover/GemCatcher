using Godot;

public partial class Gem : Area2D
{
	[Export] float _movementSpeed = 100;
	[Export] float _minSpeedVariation = 1.0f;
	[Export] float _maxSpeedVariation = 3.5f;
	[Export] float _rotationSpeed = 15.0f;
	[Export] float _minRotationVariation = -0.5f;
	[Export] float _maxRotationVariation = 0.5f;

	[Export] private Paddle paddle;
	
	private float _speedVariation;
	private float _rotationVariation;

	public override void _Ready()
	{
		SetVariations();
		SubscribeToSignals();
	}

	public override void _Process(double delta)
	{
		HandleMovement((float)delta);
	}

		private void HandleMovement(float delta)
	{
		HandlePosition(delta);
		HandleRotation(delta);
	}

	private void HandlePosition(float delta)
	{
		var viewportBoundaryMargin = 75;

		if (Position.Y > GetViewportRect().End.Y + viewportBoundaryMargin)
		{
			QueueFree();
		}

		Position = new Vector2(Position.X, Position.Y + (_movementSpeed * _speedVariation * (float)delta));
	}

	private void HandleRotation(float delta)
	{
		Rotation += _rotationSpeed * _rotationVariation * delta;
	}

	private void SetVariations()
	{
		_speedVariation = Helper.GetRandomFloat(_minSpeedVariation, _maxSpeedVariation);

		_rotationVariation = Helper.GetRandomFloat(_minRotationVariation, _maxRotationVariation);
	}

	private void SubscribeToSignals()
	{
		AreaEntered += OnAreaEntered;
	}

	#region Signals

	private void OnAreaEntered(Area2D area)
	{
		if (area is Paddle)
		{
			QueueFree();
		}
	}

	#endregion
}
