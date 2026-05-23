using Godot;

public partial class GameOverScreen : Control
{
	[Export] Panel _gameOverPanel;

	public override void _Ready()
	{
		_gameOverPanel.Visible = false;

		SubscribeToSignals();
	}

	public override void _ExitTree()
	{
		UnsubscribeToSignals();
	}

  private void SubscribeToSignals()
  {
    SignalManager.Instance.ShowGameOverScreen += OnShowGameOverScreen;
		SignalManager.Instance.ShowMissionFailurePanel += OnShowMissionFailurePanel;
  }

	private void UnsubscribeToSignals()
  {
    SignalManager.Instance.ShowGameOverScreen -= OnShowGameOverScreen;
		SignalManager.Instance.ShowMissionFailurePanel -= OnShowMissionFailurePanel;
  }

  private void OnShowMissionFailurePanel()
  {
    _gameOverPanel.Visible = true;
  }

  private void OnShowGameOverScreen()
  {
    Visible = true;
  }
}
