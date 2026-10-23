using Godot;

public partial class Gem : Area2D, IPoolItem
{
	[Export] CpuParticles2D _particles;
	[Export] float _movementSpeed = 100;
	[Export] float _minSpeedVariation = 1.0f;
	[Export] float _maxSpeedVariation = 3.5f;
	[Export] float _rotationSpeed = 15.0f;
	[Export] float _minRotationVariation = -0.5f;
	[Export] float _maxRotationVariation = 0.5f;
	[Export] protected float _minScaleVariation = 0.5f;
	[Export] protected float _maxScaleVariation = 0.75f;

	protected int _viewportBoundaryMargin = 75;

	private Color _color;
	protected bool _isActive = false;
	protected bool _isOffScreen = false;
	private float _speedVariation;
	private float _rotationVariation;
	private float _scaleVariation;

	public float SpeedVariation
	{
		get 
		{
			return _speedVariation; 
		}
		set
		{
			_speedVariation = value;
		}
	}

	public override void _Ready()
	{
		SetVariations();
		SetScale();
		SetColor();
		SetGemSpeedByDifficultyLevel();
		BeginParticleEmission();
		SubscribeToSignals();
	}

	public override void _Process(double delta)
	{
		if (_isActive)
		{
			HandleMovement((float)delta);
		}
	}
	
	private void SubscribeToSignals()
	{
		AreaEntered += OnAreaEntered;
	}

#region IPoolItem Interface

	public void Activate()
	{
		SetActive();
		CustomUtils.ActivateArea2D(this);
		BeginParticleEmission();
		Show();
	}

	public void Deactivate()
	{
		SetInactive();
		CustomUtils.DeactivateArea2D(this);
		EndParticleEmission();
		Hide();
	}

#endregion

	private void HandleMovement(float delta)
	{
		HandlePosition(delta);
		HandleRotation(delta);
		if (!_isOffScreen)
		{
			HandleExitScreen();
		}
	}

  protected virtual void HandleExitScreen()
	{
		if (Position.Y > GetViewportRect().End.Y)
		{
			_isOffScreen = true;
			SignalManager.Instance.EmitGemOffScreen();
		}
	}

  public virtual void HandlePosition(float delta)
	{
		if (Position.Y > GetViewportRect().End.Y + _viewportBoundaryMargin)
		{
			Deactivate();
		}

		Position = new Vector2(
			Position.X,
			Position.Y + (_movementSpeed * _speedVariation * (float)delta)
		);
	}

	private void HandleRotation(float delta)
	{
		Rotation += _rotationSpeed * _rotationVariation * delta;
	}

	private void SetVariations()
	{
		_speedVariation = Helper.GetRandomFloat(_minSpeedVariation, _maxSpeedVariation);

		_rotationVariation = Helper.GetRandomFloat(_minRotationVariation, _maxRotationVariation);

		if (GameManager.Instance.CurrentGameState == (int)GameManager.GameState.VICTORY)
		{
			_maxScaleVariation = 1.15f;
		}

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

	private void SetActive()
	{
		_isActive = true;
	}

	private void SetInactive()
	{
		_isActive = false;
	}

  private void SetGemSpeedByDifficultyLevel()
  {
    SpeedVariation *= GameManager.Instance.DifficultyLevel;
  }	

	private void BeginParticleEmission()
	{
		_particles.Emitting = true;
	}

	protected void EndParticleEmission()
	{
		_particles.Emitting = false;
	}

#region Signals

	public virtual void OnAreaEntered(Area2D area)
	{
		if (area is Paddle || area is PaddleGhost)
		{
			SignalManager.Instance.EmitScored(this.Modulate);
			Deactivate();
		}
		else if (area is MenuPaddle)
		{
			Deactivate();
		}
	}

#endregion

	// TODO: Customize function to create explosion when gem is removed during stage advancement
	// private void CreateExplosion(Area2D area) {
	// 	GD.Print($"{Name} OnAreaEntered!");

	// 	var explosionPosition = _explosionMarker.GlobalPosition;

	// 	if (!_usesMarker)
	// 	{
	// 		Vector2 direction = GlobalPosition.DirectionTo(area.GlobalPosition).Normalized();
	// 		explosionPosition = GlobalPosition + direction * _explosionMargin;
	// 	}

	// 	SignalManager.EmitSpawnPoolObject(
	// 		_explosionMarker.GlobalPosition, _explosionScene
	// 	);

	// 	DeActivate();
	// }

}
