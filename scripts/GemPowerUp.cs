using Godot;

public partial class GemPowerUp : Gem
{
	const float MINIMUM_DECIBEL_LEVEL = -80;

	[Export] float _powerUpMinScaleVariation = .85f;
	[Export] float _powerUpMaxScaleVariation = 1.15f;

	[Export] Node2D _powerUpSrpite;
	[Export] AudioStreamPlayer2D _audio;
	[Export] Timer _queueFreeTimer;

	private Tween _audioVolumeFadeInTween;
	private Tween _audioVolumeFadeOutTween;
	private float _audioOriginalVolume;
	private bool _isOffBottomScreen;
	private bool _isFadingOut;

	public override void _Ready()
	{
		base._minScaleVariation = _powerUpMinScaleVariation;
		base._maxScaleVariation = _powerUpMaxScaleVariation;
		base._viewportBoundaryMargin = 1000;
		base._Ready();

		InitalizeVariables();
		SetPowerUpSpriteColor();
		SubscribeToSignals();
		CreateAudioVolumeFadeInTween();
	}

  private void SubscribeToSignals()
  {
    _queueFreeTimer.Timeout += OnQueueFreeTimerTimeout;
  }

  private void OnQueueFreeTimerTimeout()
  {
    QueueFree();
  }

  public override void _ExitTree()
	{
		base._ExitTree();
		_queueFreeTimer.Timeout -= OnQueueFreeTimerTimeout;
		KillTweens();
	}

  private void InitalizeVariables()
  {
    _audioOriginalVolume = _audio.VolumeDb;
		_audio.VolumeDb = MINIMUM_DECIBEL_LEVEL;
		_isOffBottomScreen = false;
		_isFadingOut = false;
  }

	public override void OnAreaEntered(Area2D area)
	{
		if (area is Paddle || area is PaddleGhost)
		{
			SignalManager.Instance.EmitPowerUpCollected(this.Modulate);
			SignalManager.Instance.EmitPowerUpRemoved();
			EndParticleEmission();
			QueueFree();
		}
	}

	protected override void HandleExitScreen()
	{
		if (_isOffBottomScreen || Position.Y <= GetViewportRect().End.Y)
		{
			return;
		}

		_isOffBottomScreen = true;
		SignalManager.Instance.EmitPowerUpRemoved();
		StartOffScreenRemoval();
	}

	private void SetPowerUpSpriteColor()
	{
		_powerUpSrpite.Modulate = new Color(Colors.White);
	}

	private void KillTweens()
	{
		_audioVolumeFadeInTween?.Kill();
		_audioVolumeFadeOutTween?.Kill();
	}

	private void StartOffScreenRemoval()
	{
		if (_isFadingOut)
		{
			return;
		}

		_isFadingOut = true;
		_queueFreeTimer.Start();
		CreateAudioVolumeFadeOutTween();
	}

	private void CreateAudioVolumeFadeInTween()
	{
		_audioVolumeFadeInTween = CreateTween();

		double tweenTime = 0.25;

		_audioVolumeFadeInTween.TweenProperty(
			_audio,
			"volume_db",
			_audioOriginalVolume,
			tweenTime
		).SetTrans(Tween.TransitionType.Cubic)
		.SetEase(Tween.EaseType.Out);
	}

	private void CreateAudioVolumeFadeOutTween()
	{
		_audioVolumeFadeOutTween = CreateTween();

		double tweenTime = 10;

		_audioVolumeFadeOutTween.TweenProperty(
			_audio,
			"volume_db",
			MINIMUM_DECIBEL_LEVEL,
			tweenTime
		).SetTrans(Tween.TransitionType.Cubic)
		.SetEase(Tween.EaseType.Out);
	}
}
