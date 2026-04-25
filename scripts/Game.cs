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
	[Export] private AudioStreamPlayer _explosion;

	[Export] private Node2D _heart1;
	[Export] private Node2D _heart2;
	[Export] private Node2D _heart3;

	private int _score = 0;

	public override void _Ready()
	{
		SpawnGem();
		SubscribeToSignals();
	}

	public async void OnInitiateDeathSequenceAsync()
	{
		GetTree().Paused = true;
		await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
		_explosion.Play();
		await ToSignal(GetTree().CreateTimer(1.5f), SceneTreeTimer.SignalName.Timeout);
		
		GameManager.Instance.ResetGame();
		LevelManager.Instance.LoadMainMenu();
	}

	private void SubscribeToSignals()
	{
		_gemSpawnTimer.Timeout += SpawnGem;
		SignalManager.Instance.InitiateDeathSequence += OnInitiateDeathSequenceAsync;
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

		switch (currentHealth)
		{
			case (int)HealthStatus.FullHealth:
				_heart1.Visible = true;
				_heart2.Visible = true;
				_heart3.Visible = true;
				break;
			case (int)HealthStatus.Injured:
				_heart1.Visible = false;
				_heart2.Visible = true;
				_heart3.Visible = true;
				break;
			case (int)HealthStatus.MortallyWounded:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = true;
				break;
			case (int)HealthStatus.Dead:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = false;
				break;
			default:
				_heart1.Visible = false;
				_heart2.Visible = false;
				_heart3.Visible = false;
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
