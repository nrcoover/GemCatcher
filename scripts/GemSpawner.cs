using Godot;

public partial class GemSpawner : Node2D
{
	[Export] private Timer _gemSpawnTimer;
	[Export] private PackedScene _gemScene;
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
		_gemSpawnTimer.Timeout += SpawnGem;
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

	private void SetGemPosition(Gem gem)
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
