using Godot;

[GlobalClass]
public partial class SaveGameContainerResource : Resource
{
	[Export] public InfiniteModeResource InfiniteMode { get; set; }
	[Export] public HighScoreResource HighScore { get; set; }
}
