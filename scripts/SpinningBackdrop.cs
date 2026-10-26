using Godot;

public partial class SpinningBackdrop : Node2D
{
	[Export] private AnimationPlayer _animator;
	[Export] private float _spinSpeed = 0.05f;

	public override void _Ready()
	{
		SubscribeToSignals();
		MoveToCenterOfScreen();
	}

	public override void _Process(double delta)
	{
		Rotation += _spinSpeed * (float)delta;
	}

  public override void _ExitTree()
  {
    UnsubscribeFromSignals();
  }

	private void SubscribeToSignals()
	{
		SignalManager.Instance.ShowGameOverButtons += OnShowGameOverButtons;
	}

	private void UnsubscribeFromSignals()
	{
		SignalManager.Instance.ShowGameOverButtons -= OnShowGameOverButtons;
	}

	private void OnShowGameOverButtons()
	{
		FadeInBackdrop();
	}

	private void MoveToCenterOfScreen()
	{
		var viewport = GetViewportRect();
		var viewportCenter = viewport.Size / 2;
		Position = viewportCenter;
	}

	private void FadeInBackdrop()
	{
		_animator.Play(Constants.Animations.SpinningBackdrop.FadeIn);
	}
}
