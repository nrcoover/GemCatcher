using Godot;

public partial class GemSpawner : Node2D
{
	[Export] private Timer _gemSpawnTimer;
	[Export] private PackedScene _gemScene;
	[Export] private PackedScene _gemHeartScene;
	[Export] private Node _gemContainer;
	[Export] private bool _isOnMainMenu;

	private Marker2D _leftBoundary;
	private Marker2D _rightBoundary;

	public override void _Ready()
	{
		if (_isOnMainMenu)
		{
			SetBoundaryMarkers();
		}
		else
		{
			SpawnGem();
		}

		SubscribeToSignals();
	}

  private void SetBoundaryMarkers()
  {
    _leftBoundary = (Marker2D)GetNode("../../MainMenu/CanvasLayer/MainMenuUi/MarginContainer/TitleContainer/LeftBoundary");

		_rightBoundary = (Marker2D)GetNode("../../MainMenu/CanvasLayer/MainMenuUi/MarginContainer/TitleContainer/RightBoundary");
  }

  private void SubscribeToSignals()
	{
		_gemSpawnTimer.Timeout += SpawnGemType;
	}

	private void SpawnGemType()
	{
		if (_isOnMainMenu)
		{
			SpawnGem();
			return;
		}

		var heartSpawnNumber = 10;
		var randomNumber = Helper.GetRandomInt(1, heartSpawnNumber);

		if (GameManager.Instance.GetHealth() < GameManager.Instance.MaxHealth
				&& randomNumber >= heartSpawnNumber)
		{
			SpawnHeartGem();
		}
		else
		{
			SpawnGem();			
		}
	}

	private void SpawnGem()
	{
		var gem = (Gem)_gemScene.Instantiate();
		_gemContainer.AddChild(gem);

		if (_isOnMainMenu)
		{
			SetGemPositionOnMainMenu(gem);

			return;
		}
		
		SetGemPosition(gem);
	}

	private void SpawnHeartGem()
	{
		if (_isOnMainMenu)
		{
			return;
		}

		var heartGem = (GemHeart)_gemHeartScene.Instantiate();
		_gemContainer.AddChild(heartGem);

		SetGemPosition(heartGem);
	}

	private void SetGemPosition(Node2D gem)
	{
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

	private void SetGemPositionOnMainMenu(Gem gem)
	{
		if (_leftBoundary == null || _rightBoundary == null)
		{
			SetGemPosition(gem);
			return;
		}

		var xBoundaryCoordinate = Helper.GetRandomFloat(
				_rightBoundary.GlobalPosition.X,
				_leftBoundary.GlobalPosition.X 
			);

		gem.Position = new Vector2(
				xBoundaryCoordinate,
				_leftBoundary.GlobalPosition.Y
			);
	}
}
