using Godot;

public partial class Paddle : Area2D
{
	[Export] float _movementSpeed = 200.0f;
	[Export] float _boundaryMargin = 25.0f;
	[Export] float _boostMultiplier = 1.5f;

	private Rect2 _viewportBoundary;

	public override void _Ready()
	{
		_viewportBoundary = GetViewportRect();
	}

	public override void _Process(double delta)
	{
		HandlePaddleMovement(delta);
	}

	private void HandlePaddleMovement(double delta)
	{
		HandleUserInput(delta);
		RestrictPaddleToBoundary();
	}

	private void HandleUserInput(double delta)
	{
		var noChangeInPosition = 0;
		float calculatedMovementSpeed;

		if (Input.IsActionPressed("boost"))
		{
			calculatedMovementSpeed = _movementSpeed * _boostMultiplier;
		}
		else
		{
			calculatedMovementSpeed = _movementSpeed;
		}

		if (Input.IsActionPressed("move_right"))
		{
			Position += new Vector2(
				calculatedMovementSpeed * (float)delta,
				noChangeInPosition
			);
		}

		if (Input.IsActionPressed("move_left"))
		{
			Position -= new Vector2(
				calculatedMovementSpeed * (float)delta, 
				noChangeInPosition
			);
		}
	}

	private void RestrictPaddleToBoundary()
	{
		if(Position.X < _viewportBoundary.Position.X + _boundaryMargin)
		{
			Position = new Vector2(_viewportBoundary.Position.X + _boundaryMargin, Position.Y);
		}

		if(Position.X > _viewportBoundary.End.X - _boundaryMargin)
		{
			Position = new Vector2(_viewportBoundary.End.X - _boundaryMargin, Position.Y);
		}
	}
}
