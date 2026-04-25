using System.Linq.Expressions;
using Godot;

public partial class Game : Node2D
{
	const int DEFAULT_POINT_VALUE = 1;

	[Export] private PackedScene _gemScene;
	[Export] private Timer _gemSpawnTimer;
	[Export] private Node _gemContainer;
	[Export] private Label _scoreLabel;

	private int _score = 0;

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
		IncrementScore(DEFAULT_POINT_VALUE);
		UpdateUi();
	}

	private void UpdateUi()
	{
		_scoreLabel.Text = string.Format("Score: {0:000}", _score);
	}

	private void OnGemOffScreen()
	{
		GameManager.Instance.IncrementMissedGems();
	}

	private void IncrementScore(int points)
	{
		_score += points;
	}
}
