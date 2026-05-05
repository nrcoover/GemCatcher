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
	[Export] private AudioStreamPlayer2D _scoreSound;
	[Export] private AudioStreamPlayer2D _hurtSound;

	[Export] private Node2D _heart1;
	[Export] private Node2D _heart2;
	[Export] private Node2D _heart3;

	private int _score = 0;

	public override void _Ready()
	{
		SpawnGem();
		SubscribeToSignals();
	}

	public override void _ExitTree()
	{
		UnsubscribeFromSignals();
	}

	public async void OnInitiateDeathSequenceAsync()
	{
		GetTree().Paused = true;
		await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
		_explosion.Play();
		await ToSignal(GetTree().CreateTimer(1.5f), SceneTreeTimer.SignalName.Timeout);
		
		GameManager.Instance.ResetGame();
		LevelManager.Instance.LoadMainMenu();
	}

	private void SubscribeToSignals()
	{
		_gemSpawnTimer.Timeout += SpawnGem;
		SignalManager.Instance.InitiateDeathSequence += OnInitiateDeathSequenceAsync;
		SignalManager.Instance.Scored += OnScored;
	}

	private void UnsubscribeFromSignals() {
		SignalManager.Instance.InitiateDeathSequence -= OnInitiateDeathSequenceAsync;
		SignalManager.Instance.Scored -= OnScored;
	}

	private void SpawnGem()
	{
		var gem = (Gem)_gemScene.Instantiate();
		_gemContainer.AddChild(gem);

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
		_scoreSound.Play();
		UpdateScoreUi();
	}

	private void UpdateScoreUi()
	{
		// Create Tween to update score
		// start at white
		// flash to bright white
		// fade from flash to color of captured gem
		// will need to pass the gem color to the function
		// likely need to update the signal function signature
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
		_hurtSound.Play();
		UpdateHealthUi();
	}

	private void IncrementScore(int points)
	{
		_score += points;
	}
}
