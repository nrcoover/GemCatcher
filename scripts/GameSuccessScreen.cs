using Godot;

public partial class GameSuccessScreen : Control
{
	[Export] Panel _gameSuccessPanel;
	[Export] AnimationPlayer _animator;
	[Export] TextureButton _replayButton;
	[Export] TextureButton _exitButton;
	[Export] VBoxContainer _buttonsContainer;

	public override void _Ready()
	{
		_gameSuccessPanel.Visible = false;
		_buttonsContainer.Visible = false;

		SubscribeToSignals();
	}

	public override void _ExitTree()
	{
		UnsubscribeToSignals();
	}

  private void SubscribeToSignals()
  {
    SignalManager.Instance.ShowGameSuccessScreen += OnShowGameSuccessScreen;
		SignalManager.Instance.ShowMissionSuccessPanel += OnShowMissionSuccessPanel;
		SignalManager.Instance.ShowGameSuccessButtons += OnShowGameSuccessButtons;
		_replayButton.Pressed += OnRetryButtonPressed;
		_exitButton.Pressed += OnExitButtonPressed;
  }

  private void UnsubscribeToSignals()
  {
    SignalManager.Instance.ShowGameSuccessScreen -= OnShowGameSuccessScreen;
		SignalManager.Instance.ShowMissionSuccessPanel -= OnShowMissionSuccessPanel;
		SignalManager.Instance.ShowGameSuccessButtons -= OnShowGameSuccessButtons;
  }

  private void OnShowMissionSuccessPanel()
  {
    _gameSuccessPanel.Visible = true;
		_animator.Play("show-success-panel");
  }

  private void OnShowGameSuccessScreen()
  {
    Visible = true;
  }

	private void OnShowGameSuccessButtons()
	{
		_animator.Play("show-buttons");
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}
	
  private void OnRetryButtonPressed()
  {
		SignalManager.Instance.EmitResetGame();
    LevelManager.Instance.LoadGame();
  }

  private void OnExitButtonPressed()
  {
		SignalManager.Instance.EmitResetGame();
    LevelManager.Instance.LoadMainMenu();
  }
}
