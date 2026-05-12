using Godot;

public partial class MainMenuUi : Control
{
	[Export] TextureButton _playButton;
	[Export] TextureButton _quitButton;
	[Export] Label _titleLayer1;
	[Export] Label _titleLayer2;
	[Export] Label _titleLayer3;
	[Export] Label _titleLayer4;
	[Export] Label _titleLayer5;
	[Export] Label _titleLayer6;

	public override void _Ready()
	{
		_playButton.Pressed += OnPlayClicked;
		_quitButton.Pressed += OnQuitClicked;

		InitializeTitleColors();
	}

	private void InitializeTitleColors()
	{
		_titleLayer1.Modulate = new Color(Constants.CustomColors.PurpleBright);
		_titleLayer2.Modulate = Color.FromString(Constants.CustomColors.BlueBright, Colors.White);
		_titleLayer3.Modulate = Color.FromString(Constants.CustomColors.GreenBright, Colors.White);
		_titleLayer4.Modulate = Color.FromString(Constants.CustomColors.YellowBright, Colors.White);
		_titleLayer5.Modulate = Color.FromString(Constants.CustomColors.OrangeBright, Colors.White);
		_titleLayer6.Modulate = Color.FromString(Constants.CustomColors.RedBright, Colors.White);
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
