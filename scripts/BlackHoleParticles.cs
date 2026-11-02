using Godot;

[Tool]
public partial class BlackHoleParticles : CpuParticles2D
{
	[Export]
	public CollisionShape2D CollisionShape { get; set; }

	[Export]
	public float CustomEmissionRadius { get; set; } = 0.0f;

	public override void _Ready()
	{
		if (CollisionShape?.Shape is not CircleShape2D circle)
		{
			GD.PushWarning("BlackHoleParticles: Missing CircleShape2D!");
			return;
		}

		if (CustomEmissionRadius <= 0.0f)
		{
			CustomEmissionRadius = circle.Radius;
		}

		EmissionShape = EmissionShapeEnum.SphereSurface;
		EmissionSphereRadius = CustomEmissionRadius;

		LocalCoords = true;
	}
}
