using Godot;

public partial class MenuPaddle : Area2D
{
	[Export] float PathMultiplier = 0.5f;
	[Export] float MovementSpeed = 50;
	[Export] float Scaler = 0.5f;


	private float _viewportHeight;
	private float _viewportLength;
	private float _paddlePathLenght;
	private Vector2 _paddleStartPosition;
	private Vector2 _paddleScale;
	private Marker2D _markerLeft;
	private Marker2D _markerRight;
	private int _direction = 1;

	public override void _Ready()
	{
		InstantiateVariables();
		SetProperties();
		InstantiateMarkers();
	}

	public override void _Process(double delta)
	{
		HandlePaddleMovement((float)delta);
	}

	private void InstantiateVariables()
	{
		var viewport = GetViewportRect();
		_viewportHeight =  Mathf.Abs(viewport.Position.Y - viewport.End.Y);
		_viewportLength =  Mathf.Abs(viewport.Position.X - viewport.End.X);
		_paddlePathLenght = _viewportLength * PathMultiplier;
		_paddleStartPosition = GetViewport().GetVisibleRect().Size / 2;
		_paddleScale = new Vector2(Scaler, Scaler);

		GD.Print($"Height: {_viewportHeight}; Length: {_viewportLength}; _paddleStartPosition: {_paddleStartPosition}; Path Legnth: {_paddlePathLenght}");
	}

	private void SetProperties()
	{
		Position = _paddleStartPosition;
		Scale = _paddleScale;
	}

	private void InstantiateMarkers()
	{
		_markerLeft = new Marker2D();
		_markerRight = new Marker2D();

		AddChild(_markerLeft);
		AddChild(_markerRight);

		_markerLeft.Position = new Vector2(Position.X - _paddlePathLenght/2, Position.Y);
		_markerRight.Position = new Vector2(Position.X + _paddlePathLenght/2, Position.Y);

		GD.Print($"Left Marker: {_markerLeft.Position}");
		GD.Print($"Right Marker: {_markerRight.Position}");
	}

	private void HandlePaddleMovement(float delta)
	{
		if (Position.X >= _markerRight.Position.X)
		{
			_direction = -1;
		}

		if (Position.X <= _markerLeft.Position.X)
		{
			_direction = 1;
		}

		Position += new Vector2(_direction * MovementSpeed * delta, 0);
	}
}
