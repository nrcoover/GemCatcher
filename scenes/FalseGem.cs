using Godot;

public partial class FalseGem : Gem, IPoolItem
{
	public override void _Ready()
	{
		base._Ready();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}



#region Signals

	public override void OnAreaEntered(Area2D area)
	{
		if (area is Paddle || area is PaddleGhost)
		{
			Deactivate();
		}
		else if (area is MenuPaddle)
		{
			Deactivate();
		}
	}

#endregion



#region IPoolItem Interface

	public void Activate()
	{
		CustomUtils.ActivateArea2D(this);
		BeginParticleEmission();
		Show();
	}

	public void Deactivate()
	{
		Hide();
		CustomUtils.DeactivateArea2D(this);
		EndParticleEmission();
	}

#endregion


	public override void HandlePosition(float delta)
	{
		if (Position.Y > GetViewportRect().End.Y + _viewportBoundaryMargin)
		{
			Deactivate();
		}

		Position = new Vector2(
			Position.X,
			Position.Y + (_movementSpeed * _speedVariation * (float)delta)
		);
	}

	protected override void HandleExitScreen()
	{
		if (Position.Y > GetViewportRect().End.Y)
		{
			_isOffScreen = true;
		}
	}
}
