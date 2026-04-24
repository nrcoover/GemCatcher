using Godot;

public partial class Game : Node2D
{
	[Export] private PackedScene _gemScene;
	[Export] private Timer _gemSpawnTimer;
	[Export] private Node _gemContainer;

	public override void _Ready()
	{
		SpawnGem();
		SubscribeToSignals();
	}

	public override void _Process(double delta)
	{
	}

	private void SubscribeToSignals()
	{
		_gemSpawnTimer.Timeout += SpawnGem;
	}

	private void SpawnGem()
	{
		var gem = (Gem)_gemScene.Instantiate();
		_gemContainer.AddChild(gem);

		gem.OnScored += OnScored;
		gem.OnGemOffScreen += OnGemOffScreen;

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

	private void OnScored()
	{
		GD.Print("YOU SCORED!");
	}

	private void OnGemOffScreen()
	{
		GameManager.Instance.IncrementMissedGems();
	}
}
