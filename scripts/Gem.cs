using Godot;

public partial class Gem : Area2D
{
	[Export] CpuParticles2D _particles;
	[Export] float _movementSpeed = 100;
	[Export] float _minSpeedVariation = 1.0f;
	[Export] float _maxSpeedVariation = 3.5f;
	[Export] float _rotationSpeed = 15.0f;
	[Export] float _minRotationVariation = -0.5f;
	[Export] float _maxRotationVariation = 0.5f;
	[Export] float _minScaleVariation = 0.5f;
	[Export] float _maxScaleVariation = 0.75f;

	private Color _color;
	private bool _isOffScreen = false;
	private float _speedVariation;
	private float _rotationVariation;
	private float _scaleVariation;

	public override void _Ready()
	{
		SetVariations();
		SetScale();
		SetColor();
		SubscribeToSignals();
	}

	public override void _Process(double delta)
	{
		HandleMovement((float)delta);
	}

	private void SubscribeToSignals()
	{
		AreaEntered += OnAreaEntered;
	}

	private void HandleMovement(float delta)
	{
		HandlePosition(delta);
		HandleRotation(delta);
		if (!_isOffScreen)
		{
			HandleExitScreen();
		}
	}

	private void HandleExitScreen()
	{
		if (Position.Y > GetViewportRect().End.Y)
		{
			_isOffScreen = true;
			SignalManager.Instance.EmitGemOffScreen();
		}
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

		_scaleVariation = Helper.GetRandomFloat(_minScaleVariation, _maxScaleVariation);
	}

	private void SetScale()
	{
		Scale = new Vector2(_scaleVariation, _scaleVariation);
	}

	private void SetColor()
	{
		_color = Helper.GetColorFromRainbow();
		this.Modulate = _color;
		_particles.Modulate = _color;
	}

	#region Signals

	public void OnAreaEntered(Area2D area)
	{
		if (area is Paddle)
		{
			SignalManager.Instance.EmitScored(this.Modulate);
			QueueFree();
		}
		else if (area is MenuPaddle)
		{
			QueueFree();
		}
	}

	#endregion
}
