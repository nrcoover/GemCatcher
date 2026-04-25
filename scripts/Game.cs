using System.Linq.Expressions;
using Godot;

public partial class Game : Node2D
{
	const int DEFAULT_POINT_VALUE = 1;

	enum HealthStatus
	{
		FullHealth = 3,
		Injured = 2,
		MortallyWounded = 1,
		Dead = 0
	}

	[Export] private PackedScene _gemScene;
	[Export] private Timer _gemSpawnTimer;
	[Export] private Node _gemContainer;
	[Export] private Label _scoreLabel;

	[Export] private Node2D _heart1;
	[Export] private Node2D _heart2;
	[Export] private Node2D _heart3;

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
		UpdateScoreUi();
	}

	private void UpdateScoreUi()
	{
		_scoreLabel.Text = string.Format("Score: {0:000}", _score);
	}

	private void UpdateHealthUi()
	{
		var currentHealth = GameManager.Instance.GetHealth();
		GD.Print($"HEALTH UI UPDATE CALLED! Current Health {currentHealth}");

		switch (currentHealth)
		{
			case (int)HealthStatus.FullHealth:
				_heart1.Visible = true;
				_heart2.Visible = true;
				_heart3.Visible = true;
				GD.Print("Full Health!" + currentHealth);
				break;
			case (int)HealthStatus.Injured:
				_heart1.Visible = false;
				_heart2.Visible = true;
				_heart3.Visible = true;
				GD.Print("Injured!" + currentHealth);
				break;
			case (int)HealthStatus.MortallyWounded:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = true;
				GD.Print("Mortally Wounded!" + currentHealth);
				break;
			case (int)HealthStatus.Dead:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = false;
				GD.Print("Dead!" + currentHealth);
				break;
			default:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = false;
				GD.Print("Very Dead!" + currentHealth);
				break;
		}
	}

	private void OnGemOffScreen()
	{
		GameManager.Instance.IncrementMissedGems();
		UpdateHealthUi();
	}

	private void IncrementScore(int points)
	{
		_score += points;
	}
}
