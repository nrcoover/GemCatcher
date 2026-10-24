using Godot;

public partial class GemExplosion : Node2D, IPoolItem
{
	[Export] private CpuParticles2D _particles;

	public override void _Ready()
	{
		SubscribeToSignals();
	}

	public void Activate()
	{
		_particles.Restart();
	}

	public void Deactivate()
	{
		Hide();
	}

	private void SubscribeToSignals()
	{
		_particles.Finished += Deactivate;
	}
}
