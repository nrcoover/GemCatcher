using Godot;

public partial class GameOverScreen : Control
{
	[Export] Panel _gameOverPanel;
	[Export] AnimationPlayer _animator;
	[Export] TextureButton _retryButton;
	[Export] TextureButton _exitButton;
	[Export] VBoxContainer _buttonsContainer;

	public override void _Ready()
	{
		_gameOverPanel.Visible = false;
		_buttonsContainer.Visible = false;

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
		SignalManager.Instance.ShowGameOverButtons += OnShowGameOverButtons;
		_retryButton.Pressed += OnRetryButtonPressed;
		_exitButton.Pressed += OnExitButtonPressed;
  }

  private void UnsubscribeToSignals()
  {
    SignalManager.Instance.ShowGameOverScreen -= OnShowGameOverScreen;
		SignalManager.Instance.ShowMissionFailurePanel -= OnShowMissionFailurePanel;
		SignalManager.Instance.ShowGameOverButtons -= OnShowGameOverButtons;
  }

  private void OnShowMissionFailurePanel()
  {
    _gameOverPanel.Visible = true;
		_animator.Play("flash");
  }

  private void OnShowGameOverScreen()
  {
    Visible = true;
  }

	private void OnShowGameOverButtons()
	{
		_animator.Play("show-buttons");
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}
	
  private void OnRetryButtonPressed()
  {
    LevelManager.Instance.LoadGame();
  }

  private void OnExitButtonPressed()
  {
    LevelManager.Instance.LoadMainMenu();
  }
}
