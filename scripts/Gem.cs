using Godot;

public partial class Gem : Area2D
{
	[Export] CpuParticles2D _particles;
	[Export] protected PackedScene _explosionScene;
	[Export] private Paddle _paddle;
	[Export] protected float _movementSpeed = 100;
	[Export] float _minSpeedVariation = 1.0f;
	[Export] float _maxSpeedVariation = 3.5f;
	[Export] float _rotationSpeed = 15.0f;
	[Export] float _minRotationVariation = -0.5f;
	[Export] float _maxRotationVariation = 0.5f;
	[Export] protected float _minScaleVariation = 0.5f;
	[Export] protected float _maxScaleVariation = 0.75f;

	protected int _viewportBoundaryMargin = 75;

	private Color _color;
	protected bool _isOffScreen = false;
	protected bool _isInEventHorizon = false;
	private float _wavePhase = 0.0f;
	protected bool _isGameGem = false;
	protected float _speedVariation;
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
		SubscribeToSignals();

		SetVariations();
		SetScale();
		SetColor();
		SetGemSpeedByDifficultyLevel();
		BeginParticleEmission();

		if (GetTree().CurrentScene.Name == "Game")
		{
			_isGameGem = true;
		}

		if (_isGameGem)
		{
			GetReferenceToPlayerPaddle();
		}
	}

	public override void _Process(double delta)
	{
		HandleMovement((float)delta);
	}
	
	public override void _ExitTree()
	{
		UnsubscribeFromSignals();
	}



#region Signals

	private void SubscribeToSignals()
	{
		SignalManager.Instance.AdvanceStage += OnAdvanceStage;
		SignalManager.Instance.GameVictory += OnGameVictory;
		SignalManager.Instance.BlackHoleActivated += OnBlackHoleActivated;
		SignalManager.Instance.PlayerSummonedExplosion += OnPlayerSummonedExplosion;
		AreaEntered += OnAreaEntered;
		AreaExited += OnAreaExited;
	}

	private void UnsubscribeFromSignals()
	{
		SignalManager.Instance.AdvanceStage -= OnAdvanceStage;
		SignalManager.Instance.GameVictory -= OnGameVictory;
		SignalManager.Instance.BlackHoleActivated -= OnBlackHoleActivated;
		SignalManager.Instance.PlayerSummonedExplosion -= OnPlayerSummonedExplosion;
	}

	private void OnAdvanceStage()
	{
		CreateExplosion();
	}

	private void OnGameVictory()
	{
		CreateExplosion();
	}

	private async void OnBlackHoleActivated()
	{
		CheckIfInBlackHole();
	}

	private void OnPlayerSummonedExplosion()
	{
		CreateExplosion();
	}

	public virtual void OnAreaEntered(Area2D area)
	{
		// through inheritance, Heart and PowerUp gems are intentionally unaffected by the event horizon
		if (area is BlackHole)
		{
			_isInEventHorizon = true;
		}
		else if (area is Paddle || area is PaddleGhost)
		{
			SignalManager.Instance.EmitScored(Modulate);
			HandleObjectRemoval();
		}
		else if (area is MenuPaddle)
		{
			HandleObjectRemoval();
		}
	}

	private void OnAreaExited(Area2D area)
	{
		if (area is BlackHole)
		{
			_isInEventHorizon = false;
		}
	}

#endregion



#region Handlers

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
			HandleObjectRemoval();
		}

		if (_isGameGem && _isInEventHorizon)
		{
			GetPositionInEventHorizon(delta);
		}
		else
		{
			Position = new Vector2(
				Position.X,
				Position.Y + (_movementSpeed * SpeedVariation * (float)delta)
			);
		}
	}

	private void GetPositionInEventHorizon(float delta)
	{
		var percentDistanceTraveled = GetPercentDistanceFromSingularity();

		var movementSpeed = GetMovementSpeed(percentDistanceTraveled);
		var direction = GetMovementDirection();
		var perpendicularDirection = GetPerpendicularDirection(direction);
    var movement = direction * movementSpeed * delta;

    GlobalPosition += movement;

    var waveOffset = GetWaveOffset(percentDistanceTraveled, delta);

		// Apply oscillation
    GlobalPosition += perpendicularDirection * waveOffset;
	}

	private void HandleRotation(float delta)
	{
		Rotation += _rotationSpeed * _rotationVariation * delta;
	}

	protected virtual void HandleObjectRemoval()
	{
		EndParticleEmission();
		QueueFree();
	}

#endregion



#region Event Horizon Methods

	private float GetPercentDistanceFromSingularity()
	{
		var defaultValue = 0.0f;

		var radius = _paddle.EventHorizonRadius;
		var distanceToPaddle = GlobalPosition.DistanceTo(_paddle.GlobalPosition);

		if(radius == defaultValue || distanceToPaddle == defaultValue)
		{
			return defaultValue;
		}

		return Helper.GetReverseDecimalPercentRemaining(radius, distanceToPaddle);
	}

	private void GetReferenceToPlayerPaddle()
	{
		_paddle = GetNode<Paddle>("/root/Game/Paddle");

		if(_paddle == null)
		{
			GD.PrintErr($"{this.Name} Scene: No Paddle Found in Scene!");
		}
	}

	private float GetMovementSpeed(float percentAlongPath)
	{
		const float MAX_SPEED_IN_EVENT_HORIZON_MAX_SPEED = 600.0f;
		
		var currentSpeed = _movementSpeed * SpeedVariation;
		var adjustedSpeed = MAX_SPEED_IN_EVENT_HORIZON_MAX_SPEED * percentAlongPath;

		if (adjustedSpeed < currentSpeed)
		{
			adjustedSpeed = currentSpeed;
		}

		return adjustedSpeed;
	}

	private Vector2 GetMovementDirection()
	{
		return (_paddle.GlobalPosition - GlobalPosition).Normalized();
	}

	private Vector2 GetPerpendicularDirection(Vector2 direction)
	{
		return new Vector2
		(
			-direction.Y,
			direction.X
		);
	}

	private float GetWaveOffset(float travelProgress, float delta)
	{
		const float MAX_WAVE_AMPLITUDE = 20.0f;
    const float MAX_WAVE_FREQUENCY = 10.0f;

		var completeDistanceRepresentation = 1.0f;
    var percentDistanceRemaining = completeDistanceRepresentation - travelProgress;

		var adjustedWaveAmplitude = MAX_WAVE_AMPLITUDE * percentDistanceRemaining;
    var adjustedWaveFrequency = MAX_WAVE_FREQUENCY * percentDistanceRemaining;

    _wavePhase += Mathf.Tau * adjustedWaveFrequency * delta;

    return Mathf.Sin(_wavePhase) * adjustedWaveAmplitude;
	}

	private void CheckIfInBlackHole()
	{
		if (_paddle == null || !IsInstanceValid(_paddle))
		{
			return;
		}

		float distance = GlobalPosition.DistanceTo(_paddle.GlobalPosition);

		_isInEventHorizon = distance <= _paddle.EventHorizonRadius;
	}

#endregion



#region Initalizers

	private void SetVariations()
	{
		SpeedVariation = Helper.GetRandomFloat(_minSpeedVariation, _maxSpeedVariation);

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
		Modulate = _color;
		_particles.Modulate = _color;
	}

  private void SetGemSpeedByDifficultyLevel()
  {
    SpeedVariation *= GameManager.Instance.DifficultyLevel;
  }

#endregion



#region Particles & Effects

	protected void BeginParticleEmission()
	{
		_particles.Emitting = true;
	}

	protected void EndParticleEmission()
	{
		_particles.Emitting = false;
	}

	protected virtual void CreateExplosion() {
		var colorHex = Helper.GetHexCodeFromColor(Modulate);

		SignalManager.EmitSpawnPoolObject(
			GlobalPosition, _explosionScene, colorHex
		);

		CallDeferred(MethodName.HandleObjectRemoval);
	}

	// TODO: Fix bug in which last gem captured before stage advancement also explodes
#endregion
}
