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
		_titleLayer1.Modulate = new Color(Constants.CustomColors.PurplePastelle);
		_titleLayer2.Modulate = new Color(Constants.CustomColors.BluePastelle);
		_titleLayer3.Modulate = new Color(Constants.CustomColors.GreenPastelle);
		_titleLayer4.Modulate = new Color(Constants.CustomColors.YellowPastelle);
		_titleLayer5.Modulate = new Color(Constants.CustomColors.OrangePastelle);
		_titleLayer6.Modulate = new Color(Constants.CustomColors.RedPastelle);
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
