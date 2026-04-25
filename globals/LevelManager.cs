using Godot;

public partial class LevelManager : Node
{
	public static LevelManager Instance { get; private set;}

	private PackedScene Game;
	private PackedScene MainMenu;

	public override void _Ready()
	{
		Instance ??= this;
		LoadLevels();
	}

	public void LoadGame()
	{
		GetTree().ChangeSceneToPacked(Game);
	}

	public void LoadMainMenu()
	{
		GetTree().ChangeSceneToPacked(MainMenu);
		GetTree().Paused = false;
	}

	public void QuitGame()
	{
		GetTree().Quit();
	}

	private void LoadLevels()
	{
		Game = GD.Load<PackedScene>("res://scenes/Game.tscn");
		MainMenu = GD.Load<PackedScene>("res://scenes/MainMenu.tscn");
	}
}
