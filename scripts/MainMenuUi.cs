using Godot;

public partial class MainMenuUi : Control
{
	[Export] TextureButton PlayButton;
	[Export] TextureButton QuitButton;

	public override void _Ready()
	{
		PlayButton.Pressed += OnPlayClicked;
		QuitButton.Pressed += OnQuitClicked;
	}

  private void OnPlayClicked()
  {
    LevelManager.Instance.LoadGame();
  }

	private void OnQuitClicked()
  {
    LevelManager.Instance.QuitGame();
  }
}
