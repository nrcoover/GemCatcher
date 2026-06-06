using Godot;

public partial class GemPowerUp : Gem
{
	[Export] float _powerUpMinScaleVariation = .85f;
	[Export] float _powerUpMaxScaleVariation = 1.15f;

	[Export] Node2D _powerUpSrpite;

	public override void _Ready()
	{
		base._minScaleVariation = _powerUpMinScaleVariation;
		base._maxScaleVariation = _powerUpMaxScaleVariation;
		base._Ready();

		SetPowerUpSpriteColor();
	}

	public override void OnAreaEntered(Area2D area)
	{
		if (area is Paddle)
		{
			SignalManager.Instance.EmitPowerUpCollected();
			EndParticleEmission();
			QueueFree();
		}
	}

	protected override void HandleExitScreen()
	{
		if (Position.Y > GetViewportRect().End.Y)
		{
			base._isOffScreen = true;
		}
	}

	private void SetPowerUpSpriteColor()
	{
		_powerUpSrpite.Modulate = new Color(Colors.White);
	}
}
