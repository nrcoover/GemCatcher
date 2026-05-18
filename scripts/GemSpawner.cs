using Godot;

public partial class GemSpawner : Node2D
{
	[Export] private Timer _gemSpawnTimer;
	[Export] private PackedScene _gemScene;
	[Export] private PackedScene _uiGemScene;
	[Export] private Node _gemContainer;
	[Export] private bool _isOnMainMenu;

	[Export] private Node2D _layer1;
	[Export] private Node2D _layer2;
	[Export] private Node2D _layer3;
	[Export] private Node2D _layer4;
	[Export] private Node2D _layer5;
	[Export] private Node2D _layer6;

	private Marker2D _leftBoundary;
	private Marker2D _rightBoundary;

	public override void _Ready()
	{
		if (_isOnMainMenu)
		{
			SetBoundaryMarkers();
		}

		SpawnGem();
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
		
		if (_isOnMainMenu)
		{
			SetGemPositionOnMainMenu(gem);

			return;
		}

		_gemContainer.AddChild(gem);
		
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
			GD.Print("Marker Node Path Not Found!");
			SetGemPosition(gem);
			return;
		}

		SetZIndexOnMainMenu(gem);

		var xBoundaryCoordinate = Helper.GetRandomFloat(
				_rightBoundary.GlobalPosition.X,
				_leftBoundary.GlobalPosition.X 
			);

		gem.Position = new Vector2(
				xBoundaryCoordinate,
				_leftBoundary.GlobalPosition.Y
			);
	}

	private void SetZIndexOnMainMenu(Gem gem)
	{
		var color = gem.Modulate;

		// gem.ZIndex = Helper.SetZIndexByColor(gem.Modulate);

		// GD.Print($"COLOR: {color}");

		var redPastell = new Color (Constants.CustomColors.RedPastelle);
		var orangePastelle = new Color (Constants.CustomColors.OrangePastelle);
		var yellowPastelle = new Color (Constants.CustomColors.YellowPastelle);
		var greenPastelle = new Color (Constants.CustomColors.GreenPastelle);
		var blueLightPastelle = new Color (Constants.CustomColors.BlueLightPastelle);
		var bluePastelle = new Color (Constants.CustomColors.BluePastelle);
		var blueDarkPastelle = new Color (Constants.CustomColors.BlueDarkPastelle);
		var purplePastelle = new Color (Constants.CustomColors.PurplePastelle);
		var pinkPastelle = new Color (Constants.CustomColors.PinkPastelle);

		if (color == redPastell)
		{
			GD.Print("RED DISCOVERED!");
			_layer6.AddChild(gem);
		}
		else if (color == orangePastelle)
		{
			_layer5.AddChild(gem);
		}
		else if (color == yellowPastelle)
		{
			_layer4.AddChild(gem);
		}
		else if (color == greenPastelle)
		{
			_layer3.AddChild(gem);
		}
		else if (color == blueLightPastelle)
		{
			_layer2.AddChild(gem);
		}
		else if (color == bluePastelle)
		{
			_layer2.AddChild(gem);
		}
		else if (color == blueDarkPastelle)
		{
			_layer2.AddChild(gem);
		}
		else if (color == purplePastelle)
		{
			_layer1.AddChild(gem);
		}
		else if (color == pinkPastelle)
		{
			_gemContainer.AddChild(gem);
		}
		else
		{
			_gemContainer.AddChild(gem);
		}
	}
}
