using System;
using Godot;

public partial class GemPowerUp : Gem
{
	[Export] float _powerUpMinScaleVariation = .85f;
	[Export] float _powerUpMaxScaleVariation = 1.15f;

	[Export] Node2D _powerUpSrpite;
	[Export] AudioStreamPlayer2D _audio;

	private Tween _audioVolumeTween;
	private float _audioOriginalVolumne;
	private float _fadeInVolumne = -80;

	public override void _Ready()
	{
		base._minScaleVariation = _powerUpMinScaleVariation;
		base._maxScaleVariation = _powerUpMaxScaleVariation;
		base._Ready();

		InitalizeVariables();
		SetPowerUpSpriteColor();
		CreateAudioVolumeTween();
	}

  public override void _ExitTree()
	{
		KillTweens();
	}

  private void InitalizeVariables()
  {
    _audioOriginalVolumne = _audio.VolumeDb;
		_audio.VolumeDb = _fadeInVolumne;
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
		if (Position.Y > GetViewportRect().End.Y)
		{
			base._isOffScreen = true;
			SignalManager.Instance.EmitPowerUpRemoved();
		}
	}

	private void SetPowerUpSpriteColor()
	{
		_powerUpSrpite.Modulate = new Color(Colors.White);
	}

	private void KillTweens()
	{
		_audioVolumeTween?.Kill();
	}

	private void CreateAudioVolumeTween()
	{
		_audioVolumeTween = CreateTween();

		var tweenTime = 0.25f;

		_audioVolumeTween.TweenProperty(
			_audio,
			"volume_db",
			_audioOriginalVolumne,
			tweenTime
		).SetTrans(Tween.TransitionType.Cubic)
		.SetEase(Tween.EaseType.Out);
	}
}
