using Godot;

[GlobalClass]
public partial class GemButton : TextureButton
{
	[Export] public GpuParticles2D _particles;
	[Export] public float _flashSpeed = 0.35f; 
	[Export] public AudioStreamPlayer2D _sparkleSound;
    
	private Tween _spinTween;
	private Color _originalColor;

	public override void _Ready()
	{
		SubscribeToSignals();

		SetPivotPoint();
		_particles.Emitting = true;
		SetParticleEmission(false);

		_originalColor = SelfModulate;
	}

	public void KillSpinTwin()
	{
		_spinTween?.Kill();
	}

	protected virtual void SubscribeToSignals()
	{
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
	}

	private void SetPivotPoint()
	{
		PivotOffset = Size/2;
	}

	private void OnMouseEntered()
	{
		SetParticleEmission(true);

		if (Disabled)
		{
			SelfModulate = new Color(
				Helper.GetColorFromHexCode(Constants.CustomColors.DisabledGray)
			);

			return;
		}
		else
		{
			SelfModulate = _originalColor;
		}

		PlaySparkleSound();

		_spinTween = CreateTween();
		Color flashColor = _originalColor.Lightened(0.65f);
		
		_spinTween.TweenProperty(
			this,
			PropertyName.SelfModulate.ToString(),
			flashColor,
			_flashSpeed
		);

		_spinTween.TweenProperty(
			this,
			PropertyName.SelfModulate.ToString(),
			_originalColor,
			_flashSpeed
		);
							
		_spinTween.SetLoops(0); 
	}

  private void OnMouseExited()
	{
		SetParticleEmission(false);
		KillSpinTwin();

		if (Disabled)
		{
			SelfModulate = new Color(
				Helper.GetColorFromHexCode(Constants.CustomColors.DisabledGray)
			);

			return;
		}
		
		Tween resetTween = CreateTween();
		resetTween.TweenProperty(
			this,
			PropertyName.SelfModulate.ToString(),
			_originalColor,
			_flashSpeed
		);
	}

  private void SetParticleEmission(bool isVisible)
  {
    _particles.Visible = isVisible;
  }

	private void PlaySparkleSound()
	{
		_sparkleSound.Play();
	}
}