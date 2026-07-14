using System.Threading.Tasks;
using Godot;

public partial class Meteor : Area2D
{
	const int METEOR_UNLOCK_STAGE = 3;
	const float MAXIMUM_STAGE_SPEED_MULTIPLIER = 1.75f;
	const float STAGE_SPEED_MULTIPLIER = 0.08f;

	[Export] private Sprite2D _sprite;
	[Export] private CollisionShape2D _collisionShape;
	[Export] private CpuParticles2D _trailParticles;
	[Export] private CpuParticles2D _impactParticles;
	[Export] private AudioStreamPlayer2D _approachAudio;
	[Export] private AudioStreamPlayer2D _impactAudio;
	[Export] private float _movementSpeed = 250.0f;
	[Export] private float _minimumSpeedVariation = 0.9f;
	[Export] private float _maximumSpeedVariation = 1.15f;
	[Export] private float _maximumHorizontalDrift = 55.0f;

	private Vector2 _velocity;
	private bool _isResolved;

	public override void _Ready()
	{
		Scale *= GameManager.Instance?.HazardScaleMultiplier ?? 1.0f;
		InitializeMovement();
		_trailParticles.Emitting = true;
		AreaEntered += OnAreaEntered;
	}

	public override void _Process(double delta)
	{
		if (_isResolved)
		{
			return;
		}

		Position += _velocity * (float)delta;

		if (Position.Y > GetViewportRect().End.Y + 110.0f)
		{
			_isResolved = true;
			SignalManager.Instance.EmitMeteorDodged();
			QueueFree();
		}
	}

	public override void _ExitTree()
	{
		AreaEntered -= OnAreaEntered;
	}

	private void InitializeMovement()
	{
		var hazardLevel = Mathf.Max(0, GameManager.Instance.CurrentStage - METEOR_UNLOCK_STAGE);
		var stageSpeedMultiplier = Mathf.Min(
			MAXIMUM_STAGE_SPEED_MULTIPLIER,
			1.0f + hazardLevel * STAGE_SPEED_MULTIPLIER
		);
		var speedVariation = Helper.GetRandomFloat(
			_minimumSpeedVariation,
			_maximumSpeedVariation
		);
		var horizontalDrift = Helper.GetRandomFloat(
			-_maximumHorizontalDrift,
			_maximumHorizontalDrift
		);

		_velocity = new Vector2(
			horizontalDrift,
			_movementSpeed * speedVariation * stageSpeedMultiplier
		);

		// The source sprite points down-right at 45 degrees.
		Rotation = _velocity.Angle() - Mathf.Pi / 4.0f;
	}

	private void OnAreaEntered(Area2D area)
	{
		if (_isResolved || area is not Paddle)
		{
			return;
		}

		ResolveImpactAsync();
	}

	private async void ResolveImpactAsync()
	{
		_isResolved = true;
		SetDeferred(Area2D.PropertyName.Monitoring, false);
		_collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		_approachAudio.Stop();
		_sprite.Visible = false;
		_trailParticles.Emitting = false;
		_impactParticles.Emitting = true;
		_impactAudio.Play();
		SignalManager.Instance.EmitMeteorHit();

		await CreateTimerAsync(1.7f);

		if (GodotObject.IsInstanceValid(this) && IsInsideTree())
		{
			QueueFree();
		}
	}

	private async Task CreateTimerAsync(float seconds)
	{
		var tree = GetTree();

		if (tree == null)
		{
			return;
		}

		await ToSignal(tree.CreateTimer(seconds, false), SceneTreeTimer.SignalName.Timeout);
	}
}
