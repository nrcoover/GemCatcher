using System.Threading.Tasks;
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
		_buttonsContainer.Visible = false;
		_gameSuccessPanel.Visible = false;
		_gameSuccessPanel.Modulate = new Color(Constants.CustomColors.AplhaInvisible);
		Modulate = new Color(Constants.CustomColors.AplhaInvisible);

		SubscribeToSignals();
	}

	public override void _ExitTree()
	{
		UnsubscribeToSignals();
	}

  private void SubscribeToSignals()
  {
		SignalManager.Instance.InitiateVictorySequence += OnInitiateVictorySequenceAsync;
		_replayButton.Pressed += OnRetryButtonPressed;
		_exitButton.Pressed += OnExitButtonPressed;
  }

  private void UnsubscribeToSignals()
  {
		SignalManager.Instance.InitiateVictorySequence -= OnInitiateVictorySequenceAsync;
  }

  private void ShowMissionSuccessPanel()
  {
    _gameSuccessPanel.Visible = true;
		PlayShowSuccessPanelAnimation();
  }

  private void ShowGameSuccessScreen()
  {
    Visible = true;
		PlayScreenFadeInAnimation();
  }

	private void ShowGameSuccessButtons()
	{
		PlayShowButtonsAnimation();
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

	private async void OnInitiateVictorySequenceAsync()
	{
		ShowGameSuccessScreen();
		
		await CreateTimerAsync(3.5f); // Above animation is 4 seconds; does this leaves a transparency by ending the animation early

		ShowMissionSuccessPanel();

		await CreateTimerAsync(4.0f);

		ShowGameSuccessButtons();
	}

	private void PlayScreenFadeInAnimation()
	{
		_animator.Play(Constants.Animations.GameSuccessScreen.FadeIn);
	}
	
	private void PlayShowSuccessPanelAnimation()
	{
		_animator.Play(Constants.Animations.GameSuccessScreen.ShowSuccessPanel);
	}

	private void PlayShowButtonsAnimation()
	{
		_animator.Play(Constants.Animations.GameSuccessScreen.ShowButtons);
	}

	#region Asynchronous Tasks

	private async Task CreateTimerAsync(float timeInSeconds)
	{
		var tree = GetTree();

		if (tree == null)
		{
			return;
		}

		await ToSignal(
			tree.CreateTimer(timeInSeconds),
			SceneTreeTimer.SignalName.Timeout
		);
	}

#endregion
}
