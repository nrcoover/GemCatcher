using Godot;

public partial class Paddle : Area2D
{
	const float MAX_BOOST_FUEL = 100.0f;

	[Export] float _movementSpeed = 200.0f;
	[Export] float _boundaryMargin = 25.0f;
	[Export] float _boostMultiplier = 1.5f;

	[Export] Label _boostLabel;
	[Export] Timer _boostRefuelTimer;
	[Export] private float _boostBurnRate;
	[Export] private float _boostRefuelRate;
	private float _boostFuel;
	private bool _boostDepleted;
	private bool _isBoosting;
	private bool _isBoostable;
	private bool _isFullyFueled;

	private Rect2 _viewportBoundary;

	public override void _Ready()
	{
		_viewportBoundary = GetViewportRect();
		SubscribeToSignals();
		UpdateBoostUi();
		InitializeVariables();
	}
  
  public override void _Process(double delta)
	{
		HandlePaddleMovement((float)delta);
		HandleFuelConsumption((float)delta);
		UpdateBoostUi();
	}

	public override void _ExitTree()
	{
		UnsubscribeFromSignals();
	}

	private void InitializeVariables()
  {
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
		SignalManager.Instance.BoostDisengaged += OnBoostDisengaged;
	}

	private void UnsubscribeFromSignals()
	{
		SignalManager.Instance.BoostFuelDepleted -= OnBoostFuelDepleted;
		SignalManager.Instance.BoostDisengaged -= OnBoostDisengaged;
	}

	private void OnRefuelTimeout()
	{
		_isBoostable = true;
		// sound effect for "boosters reengaged" (conversely "boosters depleted; cool-down in progress)
	}

	private void OnBoostDisengaged()
  {
		if (_isBoostable)
		{
    	_isBoosting = true;
		}
  }

	private void OnBoostFuelDepleted()
  {
    _boostDepleted = true;
		_isBoostable = false;
		_isBoosting = false;
		_boostRefuelTimer.Start();
		// play audio announcing fuel depletion
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
			_isBoosting = true;
			calculatedMovementSpeed = _movementSpeed * _boostMultiplier;
		}
		else
		{
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

		switch (true)
		{
			case true when _isBoosting && !_boostDepleted:
				BurnFuel(delta);
				break;

			case true when _isBoosting && _boostDepleted:
				SignalManager.Instance.EmitBoostFuelDepleted();
				break;

			case true when !_isBoosting && _boostDepleted
									|| !_isBoosting && !_boostDepleted && !_isFullyFueled:
				RefuelBoost(delta);
				break;
		}
	}

	private void BurnFuel(float delta)
	{
		_boostFuel -= _boostBurnRate * delta;
		GD.Print($"Is Burning: {_boostFuel}");
	}

	private void RefuelBoost(float delta)
	{
		_boostFuel += _boostRefuelRate * delta;

		if (_boostFuel > MAX_BOOST_FUEL)
		{
			_boostFuel = MAX_BOOST_FUEL;
			_isFullyFueled = true;
		}

		GD.Print($"Is Refueling: {_boostFuel}");
	}

#endregion

#region UI UPdates

	private void UpdateBoostUi()
	{
		_boostLabel.Text = $"Boost:\n{_boostFuel}";
	}

#endregion
}
