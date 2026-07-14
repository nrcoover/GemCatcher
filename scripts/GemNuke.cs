using Godot;

public partial class GemNuke : Gem
{
	private const float MinimumScale = 0.8f;
	private const float MaximumScale = 0.95f;

	[Export] private CpuParticles2D _nukeParticles;

	public override void _Ready()
	{
		_minScaleVariation = MinimumScale;
		_maxScaleVariation = MaximumScale;
		base._Ready();

		Modulate = Colors.White;
		_nukeParticles.Modulate = new Color(Constants.CustomColors.BlueLightBright);
	}

	public override void OnAreaEntered(Area2D area)
	{
		if (area is not Paddle && area is not PaddleGhost)
		{
			return;
		}

		GameManager.Instance.TryStoreNuke();
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
