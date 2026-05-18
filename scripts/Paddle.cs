using Godot;

public partial class Paddle : Area2D
{

	#region Variables - Debugging

	const bool IS_DEBUGGING = true;
	[Export] Label _debugLogLabel;

	#endregion

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

	[Export] AudioStreamPlayer2D _boostSound;
	[Export] private float _boostBurnRate;
	[Export] private float _boostRefuelRate = 12.5f;
	private float _boostFuel;
	private bool _isFullyFueled;
	private bool _boostLockedUntilRelease;
	private bool _boostersReady;
	private bool _isInCooldown;

	private Tween _colorScaleTween;
	
	private bool _isTryingToBoost =>
	Input.IsActionPressed("boost");

	private bool _canBoost =>
		_isTryingToBoost &&
		!_boostLockedUntilRelease &&
		_boostersReady &&
		_boostFuel > 0;

	private bool _canRefuel =>
    _boostFuel < MAX_BOOST_FUEL &&
    !_boostLockedUntilRelease;

	private Rect2 _viewportBoundary;
	
  private void HandleDebugLog()
  {
    _debugLogLabel.Text = "BEBUGGING: \n"
			+ $"Fuel: {_boostFuel},\n" 
			+ $"BoostersReady: {_boostersReady},\n"
			+ $"CanBoost: {_canBoost},\n"
			+ $"CanRefuel: {_canRefuel},\n"
			+ $"InCooldown: {_isInCooldown},\n"
			+ $"Locked: {_boostLockedUntilRelease},\n"
			+ $"Boosting: {_isTryingToBoost},\n"
			;
  }

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

		if (IS_DEBUGGING)
		{
			HandleDebugLog();
		}
	}

  public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("boost"))
		{
			if (_canBoost)
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

		if (_colorScaleTween != null)
		{
    	_colorScaleTween.Kill();
		}
	}

	private void InitializeVariables()
  {
		_viewportBoundary = GetViewportRect();
    _boostFuel = MAX_BOOST_FUEL;
		_boostLockedUntilRelease = false;
		_boostersReady = true;
		_isFullyFueled = _boostFuel >= MAX_BOOST_FUEL;
		_isInCooldown = false;
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
		SignalManager.Instance.Scored += OnScored;
		SignalManager.Instance.GameOver += OnGameOver;
	}

  private void UnsubscribeFromSignals()
	{
		SignalManager.Instance.BoostFuelDepleted -= OnBoostFuelDepleted;
		SignalManager.Instance.BoostEngaged -= OnBoostEngaged;
		SignalManager.Instance.BoostDisengaged -= OnBoostDisengaged;
		SignalManager.Instance.Scored -= OnScored;
	}

	private void OnBoostEngaged()
  {
		PlayBoostAudio();
  }

	private void OnBoostDisengaged() 
	{
		if (_boostLockedUntilRelease)
		{
			_boostLockedUntilRelease = false;
			_isInCooldown = true;

			_animator.Play("flashing_warning");
			_boostRefuelTimer.Start();

			GD.Print("BOOST LOCK RELEASED");
		}

		StopBoostAudio();

		DisengageAllParticles();
	}

	private void OnRefuelTimeout()
	{
		_boostersReady = true;
		_isInCooldown = false;

		// sound effect for "boosters reengaged" (conversely "boosters depleted; cool-down in progress)
	}

	private void OnBoostFuelDepleted()
	{
		_boostFuel = 0;

		_boostersReady = false;
		_boostLockedUntilRelease = true;
		_isInCooldown = false;

		StopBoostAudio();

		// play audio announcing fuel depletion

		_animator.Stop();

		DisengageAllParticles();
	}

	private void OnScored(Color color)
  {
    CreateColorScaleTweenAsync(color);
  }
	
  private void OnGameOver()
  {
		if (_colorScaleTween == null)
		{
			return;
		}
		
    _colorScaleTween.Kill();
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

		if (_canBoost)
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
		_isFullyFueled = _boostFuel >= MAX_BOOST_FUEL;

		if (_canBoost)
		{
			BurnFuel(delta);

			if (_boostFuel <= 0 && _boostersReady)
			{
					_boostFuel = 0;
					SignalManager.Instance.EmitBoostFuelDepleted();
			}
		}
		else
		{
			float refuelRate = _isInCooldown
				? DEFAULT_REFUEL_RATE * 0.5f
				: DEFAULT_REFUEL_RATE;

			// GD.Print($"DEFAULT: {DEFAULT_REFUEL_RATE} | RATE: {refuelRate}");

			if (_canRefuel)
			{
				RefuelBoost(delta, refuelRate);
			}
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
		if (_isInCooldown)
    {
        // TODO: Play alternative animation (not made yet...)

        return;
    }

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

		if (_canBoost && isLowOnFuel)
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
		else if (!_boostLockedUntilRelease && !_isTryingToBoost && isLowOnFuel)
		{
			if (_animator.CurrentAnimation != Constants.Animations.RefuelingYellow)
			{
				// GD.Print("REFILLING");
				_animator.Play(Constants.Animations.RefuelingYellow);
			}
		}
  }

#endregion

#region Manage Particle System

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
			// GD.Print("EARLY EXIT!!!!!!!!!!");
			DisengageAllParticles();
			return;
		}

		if (_canBoost && Input.IsActionPressed("move_right"))
		{
			// GD.Print("RIGHT PARTICLES");
			EngageLeftParticles();
		}
		else if (_canBoost && Input.IsActionPressed("move_left"))
		{
			// GD.Print("LEFT PARTICLES");
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

	private async void CreateColorScaleTweenAsync(Color color)
	{
		var tweenTime = 0.25f;
		var originalScale = Scale;
		var scaleMultiplier = 1.15f;

		_colorScaleTween = CreateTween();

		_colorScaleTween.SetParallel(true);

		_colorScaleTween.TweenProperty(
			this,
			PropertyName.Modulate.ToString(),
			color,
			tweenTime
		).SetTrans(Tween.TransitionType.Cubic)
		.SetEase(Tween.EaseType.Out);

		_colorScaleTween.TweenProperty(
			this,
			PropertyName.Scale.ToString(),
			originalScale * scaleMultiplier,
			tweenTime
		).SetTrans(Tween.TransitionType.Back)
		.SetEase(Tween.EaseType.Out);

		await ToSignal(_colorScaleTween, Tween.SignalName.Finished);

		_colorScaleTween = CreateTween();

		_colorScaleTween.SetParallel(true);

		_colorScaleTween.TweenProperty(
			this,
			PropertyName.Modulate.ToString(),
			Colors.White,
			tweenTime
		).SetTrans(Tween.TransitionType.Cubic)
		.SetEase(Tween.EaseType.Out);

		_colorScaleTween.TweenProperty(
			this,
			PropertyName.Scale.ToString(),
			originalScale,
			tweenTime
		).SetTrans(Tween.TransitionType.Back)
		.SetEase(Tween.EaseType.Out);
	}

	private void PlayBoostAudio()
	{
		if (!_isTryingToBoost)
		{
			return;
		}

		if (!_boostSound.IsPlaying())
		{
			_boostSound.Play();
		}
	}

	private void StopBoostAudio()
	{
		_boostSound.Stop();
	}
}
