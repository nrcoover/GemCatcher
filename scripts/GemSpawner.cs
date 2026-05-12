using Godot;

public partial class GemSpawner : Node2D
{
	[Export] private Timer _gemSpawnTimer;
	[Export] private PackedScene _gemScene;
	[Export] private Node _gemContainer;

	public override void _Ready()
	{
		SpawnGem();
		SubscribeToSignals();
	}

	private void SubscribeToSignals()
	{
		_gemSpawnTimer.Timeout += SpawnGem;
	}

	private void SpawnGem()
	{
		var gem = (Gem)_gemScene.Instantiate();
		_gemContainer.AddChild(gem);

		var margin = 85;

		var xBoundaryCoordinate = Helper.GetRandomFloat(
				GetViewportRect().Position.X + margin, 
				GetViewportRect().End.X - margin
			);

		gem.Position = new Vector2(
				xBoundaryCoordinate,
				-margin
			);
	}
}
