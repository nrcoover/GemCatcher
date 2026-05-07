using Godot;

public partial class Paddle : Area2D
{
	const float MAX_BOOST_FUEL = 100.0f;
	const float DEFAULT_REFUEL_RATE = 12.5f;

	enum FuelState {
		Low = 50,
		Urgent = 30,
		Emergency = 15,
	}

	[Export] float _movementSpeed = 200.0f;
	[Export] float _boundaryMargin = 25.0f;
	[Export] float _boostMultiplier = 1.5f;

	[Export] Label _boostLabel;
	[Export] Label _boostPercentageLabel;
	[Export] Timer _boostRefuelTimer;
	[Export] AnimationPlayer _animator;
	[Export] ProgressBar _progressBarLeft;
	[Export] ProgressBar _progressBarRight;
	[Export] Node2D _leftParticles;
	[Export] Node2D _rightParticles;
	[Export] private float _boostBurnRate;
	[Export] private float _boostRefuelRate = 12.5f;
	private float _boostFuel;
	private bool _boostDepleted;
	private bool _isBoostable;
	private bool _isFullyFueled;
	
	private bool _isTryingToBoost =>
	Input.IsActionPressed("boost");

	private bool _canBoost =>
		_isTryingToBoost &&
		_isBoostable &&
		!_boostDepleted &&
		_boostFuel > 0;

	private Rect2 _viewportBoundary;

	public override void _Ready()
	{
		_animator.Play(Constants.Animations.Reset);
		SubscribeToSignals();
		InitializeVariables();
		UpdateBoostUi();
		ResetParticleSystems();
	}

  public override void _Process(double delta)
	{
		HandlePaddleMovement((float)delta);
		HandleFuelConsumption((float)delta);
		UpdateBoostUi();
		HandleParticles();
	}

  public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("boost"))
		{
			if (_isBoostable && !_boostDepleted)
			{
				SignalManager.Instance.EmitBoostEngaged();
			}
		}

		if (@event.IsActionReleased("boost"))
		{
			SignalManager.Instance.EmitBoostDisengaged();
		}
	}

	public override void _ExitTree()
	{
		UnsubscribeFromSignals();
	}

	private void InitializeVariables()
  {
		_viewportBoundary = GetViewportRect();
    _boostFuel = MAX_BOOST_FUEL;
		_boostDepleted = false;
		_isBoostable = true;
		_isFullyFueled = _boostFuel >= MAX_BOOST_FUEL;
  }

	private void ResetParticleSystems()
  {
    DisengageAllParticles();

		_leftParticles.Visible = true;
		_rightParticles.Visible = true;
  }

#region Signals

	private void SubscribeToSignals()
	{
		_boostRefuelTimer.Timeout += OnRefuelTimeout;
		SignalManager.Instance.BoostFuelDepleted += OnBoostFuelDepleted;
		SignalManager.Instance.BoostEngaged += OnBoostEngaged;
		SignalManager.Instance.BoostDisengaged += OnBoostDisengaged;
	}

  private void UnsubscribeFromSignals()
	{
		SignalManager.Instance.BoostFuelDepleted -= OnBoostFuelDepleted;
		SignalManager.Instance.BoostEngaged -= OnBoostEngaged;
		SignalManager.Instance.BoostDisengaged -= OnBoostDisengaged;
	}

	private void OnRefuelTimeout()
	{
		_isBoostable = true;
		// sound effect for "boosters reengaged" (conversely "boosters depleted; cool-down in progress)
	}

	private void OnBoostDisengaged()
  {
		DisengageAllParticles();
  }

	private void OnBoostEngaged()
  {
		// play boosting sound
  }

	private void OnBoostFuelDepleted()
  {
    _boostDepleted = true;
		_isBoostable = false;
		_boostRefuelTimer.Start();
		_animator.Play("flashing_warning");
		// play audio announcing fuel depletion

		DisengageAllParticles();
  }

#endregion
  
#region Paddle Movement
	
	private void HandlePaddleMovement(float delta)
	{
		HandleUserInput(delta);
		RestrictPaddleToBoundary();
	}

	private void HandleUserInput(float delta)
	{
		var noChangeInPosition = 0;
		float calculatedMovementSpeed;

		bool canBoost =
			Input.IsActionPressed("boost") &&
			_isBoostable &&
			!_boostDepleted &&
			_boostFuel > 0;

		if (canBoost)
		{
			calculatedMovementSpeed = _movementSpeed * _boostMultiplier;
		}
		else
		{
			calculatedMovementSpeed = _movementSpeed;
		}

		if (Input.IsActionJustReleased("boost"))
		{
			SignalManager.Instance.EmitBoostDisengaged();
		}

		if (Input.IsActionPressed("move_right"))
		{
			Position += new Vector2(
				calculatedMovementSpeed * delta,
				noChangeInPosition
			);
		}

		if (Input.IsActionPressed("move_left"))
		{
			Position -= new Vector2(
				calculatedMovementSpeed * delta, 
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

#endregion

#region Manage Boost
	
	private void HandleFuelConsumption(float delta)
	{
		_boostDepleted = _boostFuel <= 0;
		_isFullyFueled = _boostFuel >= MAX_BOOST_FUEL;

		var isBurningFuel = _canBoost;
		var isRunningOnFumes = _isTryingToBoost && _boostDepleted;
		var isReadyForRefueling = !isRunningOnFumes
													 || isBurningFuel && !_isFullyFueled;

		switch (true)
		{
			case true when isBurningFuel:
				BurnFuel(delta);
				break;

			case true when isRunningOnFumes:
				SignalManager.Instance.EmitBoostFuelDepleted();
				break;

			case true when isReadyForRefueling:
				if (!_isBoostable)
				{
					var impedenceMultiplier = .5f;
					RefuelBoost(delta, DEFAULT_REFUEL_RATE * impedenceMultiplier);
				}
				else
				{
					RefuelBoost(delta);
				}
				break;
		}

		HandleFuelConsumptionAnimation();
	}

  private void BurnFuel(float delta)
	{
		_boostFuel -= _boostBurnRate * delta;

		// GD.Print($"Is Burning: {_boostFuel}");
	}

	private void RefuelBoost(float delta, float refuelRate = DEFAULT_REFUEL_RATE)
	{
		_boostFuel += refuelRate * delta;

		if (_boostFuel > MAX_BOOST_FUEL)
		{
			_boostFuel = MAX_BOOST_FUEL;
			_isFullyFueled = true;
		}

		// GD.Print($"Is Refueling: {_boostFuel}");
	}

	private void HandleFuelConsumptionAnimation()
  {
		var isLowOnFuel = _boostFuel < (int)FuelState.Low;

		if (!isLowOnFuel && _animator.IsPlaying())
		{
			_animator.Play(Constants.Animations.EndLowFuelWarning);
			return;
		}
		else if (!isLowOnFuel)
		{
			return;			
		}

		if (_isBoostable && _isTryingToBoost && isLowOnFuel)
		{
			switch (true)
			{
				case true when _boostFuel < (int)FuelState.Emergency:
					if (_animator.CurrentAnimation != Constants.Animations.FuelWarningLevel3)
					{
						_animator.Play(Constants.Animations.FuelWarningLevel3);
						// GD.Print("EMERGENCY! LOW FUEL!!!");
					}
					break;

				case true when _boostFuel < (int)FuelState.Urgent:
					if (_animator.CurrentAnimation != Constants.Animations.FuelWarningLevel2)
					{
						_animator.Play(Constants.Animations.FuelWarningLevel2);
						// GD.Print("URGENT LOW FUEL");
					}
					break;

				default:
					if (_animator.CurrentAnimation != Constants.Animations.FuelWarningLevel1)
					{
						_animator.Play(Constants.Animations.FuelWarningLevel1);
						// GD.Print("WARNING LOW FUEL");
					}
					break;
			}
		} 
		else if (_isBoostable && !_isTryingToBoost && isLowOnFuel)
		{
			if (_animator.CurrentAnimation != Constants.Animations.RefuelingYellow)
			{
				// GD.Print("REFILLING");
				_animator.Play(Constants.Animations.RefuelingYellow);
			}
		}
  }

	private void EngageRightParticles()
	{
		SetParticleEmission(_leftParticles, false);
		SetParticleEmission(_rightParticles, true);
	}

	private void EngageLeftParticles()
	{
		SetParticleEmission(_rightParticles, false);
		SetParticleEmission(_leftParticles, true);
	}

	private void DisengageAllParticles()
	{
		SetParticleEmission(_leftParticles, false);
		SetParticleEmission(_rightParticles, false);
	}

	private void HandleParticles()
	{
		if (!_canBoost)
		{
			GD.Print("EARLY EXIT!!!!!!!!!!");
			DisengageAllParticles();
			return;
		}

		if (_canBoost && Input.IsActionPressed("move_right"))
		{
			GD.Print("RIGHT PARTICLES");
			EngageLeftParticles();
		}
		else if (_canBoost && Input.IsActionPressed("move_left"))
		{
			GD.Print("LEFT PARTICLES");
			EngageRightParticles();
		}
		else
		{
			DisengageAllParticles();
		}
	}

	private void SetParticleEmission(Node node, bool isEmitting)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is CpuParticles2D cpuParticles)
			{
				cpuParticles.Emitting = isEmitting;
			}
		}
	}

#endregion

#region UI UPdates

	private void UpdateBoostUi()
	{
		_boostLabel.Text = $"Boost:\n{_boostFuel}";
		_boostPercentageLabel.Text = $"{Mathf.RoundToInt(_boostFuel)}";

		// Dividing by 2 to have the two progress bars act as 1
		// Each provides half of a full progress bar's value
		_progressBarLeft.Value = _boostFuel / 2;
		_progressBarRight.Value = _boostFuel / 2;
	}

#endregion
}
