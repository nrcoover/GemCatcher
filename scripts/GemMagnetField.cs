using Godot;

public partial class GemMagnetField : Node2D
{
	[Export] private float _innerRadius = 115.0f;
	[Export] private float _outerRadius = 710.0f;
	[Export] private float _pulseSpeed = 0.55f;

	private float _phase;

	public override void _Ready()
	{
		Deactivate();
	}

	public override void _Process(double delta)
	{
		_phase = Mathf.PosMod(_phase + (float)delta * _pulseSpeed, 1.0f);
		QueueRedraw();
	}

	public override void _Draw()
	{
		DrawPulseRings();
		DrawRotatingSegments();
	}

	public void Activate()
	{
		_phase = 0.0f;
		Visible = true;
		SetProcess(true);
		QueueRedraw();
	}

	public void Deactivate()
	{
		SetProcess(false);
		Visible = false;
	}

	private void DrawPulseRings()
	{
		for (var index = 0; index < 3; index++)
		{
			var progress = Mathf.PosMod(_phase + index / 3.0f, 1.0f);
			var radius = Mathf.Lerp(_innerRadius, _outerRadius, progress);
			var alpha = Mathf.Sin(progress * Mathf.Pi) * 0.32f;
			var color = index % 2 == 0
				? new Color(0.1f, 0.95f, 1.0f, alpha)
				: new Color(1.0f, 0.25f, 0.85f, alpha);

			DrawArc(Vector2.Zero, radius, 0.0f, Mathf.Tau, 96, color, 4.0f, true);
		}
	}

	private void DrawRotatingSegments()
	{
		var rotationOffset = _phase * Mathf.Tau;
		var color = new Color(0.65f, 0.95f, 1.0f, 0.38f);
		var radius = _outerRadius * 0.72f;

		for (var index = 0; index < 6; index++)
		{
			var startAngle = rotationOffset + index * Mathf.Tau / 6.0f;
			DrawArc(
				Vector2.Zero,
				radius,
				startAngle,
				startAngle + 0.42f,
				14,
				color,
				7.0f,
				true
			);
		}
	}
}
