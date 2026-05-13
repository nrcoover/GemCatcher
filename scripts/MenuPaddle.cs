using System;
using Godot;

public partial class MenuPaddle : Area2D
{
	[Export] float _pathMultiplier = 0.5f;
	[Export] float _movementSpeed = 50;
	[Export] float _scaler = 1.25f;

	private float _viewportHeight;
	private float _viewportLength;
	private float _paddlePathLenght;
	private float _paddlePositionOffsetY;
	private Vector2 _paddleStartPosition;
	private Vector2 _paddleScale;
	private Marker2D _markerLeft;
	private Marker2D _markerRight;
	private int _direction = 1;

	private Tween _colorScaleTween;

	public override void _Ready()
	{
		InstantiateVariables();
		SetProperties();
		InstantiateMarkers();
		SubscribeToSignals();
	}

	public override void _Process(double delta)
	{
		HandlePaddleMovement((float)delta);
	}

	public override void _ExitTree()
	{
		_colorScaleTween.Kill();
	}

	private void SubscribeToSignals()
	{
		AreaEntered += OnAreaEntered;
	}

	private void OnAreaEntered(Area2D area)
	{
		CreateColorScaleTweenAsync(area.Modulate);
	}

	private void InstantiateVariables()
	{
		var viewport = GetViewportRect();
		_viewportHeight = Mathf.Abs(viewport.Position.Y - viewport.End.Y);
		_viewportLength = Mathf.Abs(viewport.Position.X - viewport.End.X);

		_paddlePathLenght = _viewportLength * _pathMultiplier;
		_paddlePositionOffsetY = viewport.End.Y / 2.5f;
		_paddleStartPosition = GetViewport().GetVisibleRect().Size / 2;
		_paddleStartPosition = new Vector2(
				_paddleStartPosition.X,
				_paddleStartPosition.Y + _paddlePositionOffsetY
			);
		_paddleScale = new Vector2(_scaler, _scaler);
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

		_markerLeft.Position = new Vector2(Position.X - _paddlePathLenght / 2, Position.Y);
		_markerRight.Position = new Vector2(Position.X + _paddlePathLenght / 2, Position.Y);
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

		Position += new Vector2(_direction * _movementSpeed * delta, 0);
	}

	private async void CreateColorScaleTweenAsync(Color color)
	{
		var tweenTime = 0.25f;
		var originalScale = Scale;
		var scaleMultiplier = 1.15f;

		_colorScaleTween = CreateTween();

		_colorScaleTween.SetParallel(true);

		_colorScaleTween.TweenProperty(
			this,
			PropertyName.Modulate.ToString(),
			color,
			tweenTime
		).SetTrans(Tween.TransitionType.Cubic)
		.SetEase(Tween.EaseType.Out);

		_colorScaleTween.TweenProperty(
			this,
			PropertyName.Scale.ToString(),
			originalScale * scaleMultiplier,
			tweenTime
		).SetTrans(Tween.TransitionType.Back)
		.SetEase(Tween.EaseType.Out);

		await ToSignal(_colorScaleTween, Tween.SignalName.Finished);

		_colorScaleTween = CreateTween();

		_colorScaleTween.SetParallel(true);

		_colorScaleTween.TweenProperty(
			this,
			PropertyName.Modulate.ToString(),
			Colors.White,
			tweenTime
		).SetTrans(Tween.TransitionType.Cubic)
		.SetEase(Tween.EaseType.Out);

		_colorScaleTween.TweenProperty(
			this,
			PropertyName.Scale.ToString(),
			originalScale,
			tweenTime
		).SetTrans(Tween.TransitionType.Back)
		.SetEase(Tween.EaseType.Out);
	}
}
