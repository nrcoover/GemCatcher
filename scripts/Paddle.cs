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
	private bool _isBoosting;
	private bool _isBoostable;
	private bool _isFullyFueled;

	private Rect2 _viewportBoundary;

	public override void _Ready()
	{
		_animator.Play(Constants.Animations.Reset);
		SubscribeToSignals();
		InitializeVariables();
		UpdateBoostUi();
	}
  
  public override void _Process(double delta)
	{
		HandleFuelConsumption((float)delta);
		UpdateBoostUi();
	}

  public override void _PhysicsProcess(double delta)
  {
		HandlePaddleMovement((float)delta);
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
		_isBoosting = false;
		_isBoostable = true;
		_isFullyFueled = _boostFuel >= MAX_BOOST_FUEL;
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
		_isBoosting = false;
		DisengageAllParticles();
  }

	private void OnBoostEngaged()
  {
		if (_isBoostable)
		{
    	_isBoosting = true;
		}

		HandleParticles();
		// play boosting sound
  }

	private void OnBoostFuelDepleted()
  {
    _boostDepleted = true;
		_isBoostable = false;
		_isBoosting = false;
		_boostRefuelTimer.Start();
		_animator.Play("flashing_warning");
		// play audio announcing fuel depletion
  }

  private void OnLowFuelRangeEntered()
  {
    GD.Print("Low Fuel Range Entered!");
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

		if (Input.IsActionPressed("boost") 
				&& _isBoostable 
				&& !_boostDepleted
			)
		{
			SignalManager.Instance.EmitBoostEngaged();
			calculatedMovementSpeed = _movementSpeed * _boostMultiplier;
		}
		else
		{
			// TODO: Move to unhandled input of boost release?
			SignalManager.Instance.EmitBoostDisengaged();
			calculatedMovementSpeed = _movementSpeed;
		}

		if (Input.IsActionJustReleased("boost"))
		{
			_isBoosting = false;
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
		
		var isBurningFuel = _isBoosting && !_boostDepleted;
		var isRunningOnFumes = _isBoosting && _boostDepleted;
		var isReadyForRefueling = !_isBoosting && _boostDepleted
													 || !_isBoosting && !_boostDepleted && !_isFullyFueled;

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

		if (_isBoostable && _isBoosting && isLowOnFuel)
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
		else if (_isBoostable && !_isBoosting && isLowOnFuel)
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
		_leftParticles.Visible = false;
		_rightParticles.Visible = true;
	}

	private void EngageLeftParticles()
	{
		_leftParticles.Visible = true;
		_rightParticles.Visible = false;
	}

	private void DisengageAllParticles()
	{
		_leftParticles.Visible = false;
		_rightParticles.Visible = false;
	}

	private void HandleParticles()
	{
		if (!_isBoosting || !_isBoostable)
		{
			GD.Print("EARLY EXIT!!!!!!!!!!");
			return;
		}

		if (Input.IsActionPressed("move_right"))
		{
			GD.Print("RIGHT PARTICLES");
			EngageLeftParticles();
		}

		if (Input.IsActionPressed("move_left"))
		{
			GD.Print("LEFT PARTICLES");
			EngageRightParticles();
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
