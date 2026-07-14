using Godot;

public partial class BonusGem : Gem
{
	private const float MinimumScale = 0.45f;
	private const float MaximumScale = 0.55f;
	private const float VisualOpacity = 0.55f;

	private static readonly Color StardustColor = new("FFD54A");
	private static readonly Color StardustLightColor = new("FFF4B0");

	[Export] private Sprite2D _sprite;
	[Export] private CpuParticles2D _bonusParticles;
	[Export] private AnimationPlayer _sparkleAnimation;

	private float _haloPhase;

	public override void _Ready()
	{
		_minScaleVariation = MinimumScale;
		_maxScaleVariation = MaximumScale;
		base._Ready();

		_sparkleAnimation.Stop();
		Modulate = new Color(1.0f, 1.0f, 1.0f, VisualOpacity);
		_sprite.Modulate = StardustColor;
		_bonusParticles.Modulate = StardustLightColor;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		_haloPhase = Mathf.PosMod(_haloPhase + (float)delta * 1.5f, 1.0f);
		QueueRedraw();
	}

	public override void _Draw()
	{
		var pulse = (Mathf.Sin(_haloPhase * Mathf.Tau) + 1.0f) * 0.5f;
		var innerRadius = Mathf.Lerp(42.0f, 49.0f, pulse);
		var outerRadius = Mathf.Lerp(55.0f, 62.0f, pulse);

		DrawArc(
			Vector2.Zero,
			innerRadius,
			0.0f,
			Mathf.Tau,
			48,
			new Color(1.0f, 0.83f, 0.25f, 0.75f),
			4.0f,
			true
		);
		DrawArc(
			Vector2.Zero,
			outerRadius,
			_haloPhase * Mathf.Tau,
			_haloPhase * Mathf.Tau + Mathf.Pi * 1.4f,
			36,
			new Color(1.0f, 0.97f, 0.72f, 0.5f),
			3.0f,
			true
		);
	}

	public override void OnAreaEntered(Area2D area)
	{
		if (area is not Paddle && area is not PaddleGhost)
		{
			return;
		}

		SignalManager.Instance.EmitStardustCollected(StardustColor);
		EndParticleEmission();
		QueueFree();
	}

	protected override void HandleExitScreen()
	{
		if (Position.Y > GetViewportRect().End.Y)
		{
			_isOffScreen = true;
		}
	}
}
