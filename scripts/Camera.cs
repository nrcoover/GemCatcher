using Godot;

public partial class Camera : Camera2D
{
	[Export] float _shakeIntensity = 0.0f;
	[Export] float _activeShakeTime = 0.0f;
	[Export] float _shakeDecay = 0.0f;
	[Export] float _shakeTime = 0.0f;
	[Export] float _shakeTimeSpeed = 20.0f;

	private bool _isRampShaking = false;
	private float _rampMinIntensity = 0.0f;
	private float _rampMaxIntensity = 0.0f;
	private float _rampDuration = 0.0f;
	private float _rampElapsed = 0.0f;

	FastNoiseLite noise = new FastNoiseLite();

	public override void _Process(double delta)
	{
		if (_activeShakeTime > 0.0f)
		{
			_shakeTime += (float)(delta * _shakeTimeSpeed);
			_activeShakeTime -= (float)delta;

			if (_isRampShaking)
			{
				_rampElapsed += (float)delta;

				float progress = Mathf.Clamp(_rampElapsed / _rampDuration, 0.0f, 1.0f);

				_shakeIntensity = Mathf.Lerp(
					_rampMinIntensity,
					_rampMaxIntensity,
					progress
				);

				if (progress >= 1.0f)
				{
					_isRampShaking = false;
				}
			}
			else
			{
				_shakeIntensity = Mathf.Max(
					_shakeIntensity - _shakeDecay * (float)delta,
					0.0f
				);
			}

			Offset = new Vector2(
				noise.GetNoise2D(_shakeTime, 0) * _shakeIntensity,
				noise.GetNoise2D(0, _shakeTime) * _shakeIntensity
			);
		}
		else
		{
			var lerpSpeed = 10.5f * delta;
			Offset = Offset.Lerp(Vector2.Zero, (float)lerpSpeed);

			_isRampShaking = false;
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

	public void RampScreenShake(float shakeTime, float minIntensity, float maxIntensity)
	{
		GD.Randomize();
		noise.Seed = (int)GD.Randi();
		noise.Frequency = 2.0f;

		_isRampShaking = true;

		_rampMinIntensity = minIntensity;
		_rampMaxIntensity = maxIntensity;
		_rampDuration = shakeTime;
		_rampElapsed = 0.0f;

		_shakeIntensity = minIntensity;

		_activeShakeTime = shakeTime;
		_shakeTime = 0.0f;
	}
}
