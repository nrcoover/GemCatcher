using Godot;

public partial class GemHeart : Gem
{
	[Export] float _heartMinScaleVariation = 1f;
	[Export] float _heartMaxScaleVariation = 1.25f;

	[Export] Node2D _heartSrpite;

	public override void _Ready()
	{
		base._minScaleVariation = _heartMinScaleVariation;
		base._maxScaleVariation = _heartMaxScaleVariation;
		base._Ready();

		SetHeartSpriteColor();
	}

	public override void OnAreaEntered(Area2D area)
	{
		if (area is Paddle)
		{
			SignalManager.Instance.EmitHealthRecovered();
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

	private void SetHeartSpriteColor()
	{
		_heartSrpite.Modulate = new Color(Constants.CustomColors.RedBright);
	}
}
