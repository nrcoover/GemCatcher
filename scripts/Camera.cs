using Godot;

public partial class Camera : Camera2D
{
	[Export] float _shakeIntensity = 0.0f;
	[Export] float _activeShakeTime = 0.0f;
	[Export] float _shakeDecay = 0.0f;
	[Export] float _shakeTime = 0.0f;
	[Export] float _shakeTimeSpeed = 20.0f;

	FastNoiseLite noise = new FastNoiseLite();

	public override void _Process(double delta)
	{
		if (_activeShakeTime > 0.0f)
		{
			_shakeTime += (float)(delta * _shakeTimeSpeed);
			_activeShakeTime -= (float)delta;

			this.Offset = new Vector2(
					  noise.GetNoise2D(_shakeTime, 0) * _shakeIntensity,
					  noise.GetNoise2D(0, _shakeTime) * _shakeIntensity
				  );

			_shakeIntensity = Mathf.Max(_shakeIntensity - _shakeDecay * (float)delta, 0.0f);
		}
		else
		{
			var lerpSpeed = 10.5f * delta;
			this.Offset = this.Offset.Lerp(Vector2.Zero, (float)lerpSpeed);
		}
	}

	public void ScreenShake(int intensity, float time)
	{
		GD.Randomize();
		noise.Seed = (int)GD.Randi();
		noise.Frequency = 2.0f;

		_shakeIntensity = intensity;
		_activeShakeTime = time;
		_shakeTime = 0.0f;
	}
}
