using Godot;

public static partial class Helper
{
	public static int GetRandomInt(int min, int max)
	{
		var randomNumber = new RandomNumberGenerator();
		randomNumber.Randomize();

		int randomInt = randomNumber.RandiRange(min, max);
		
		return randomInt;
	}

	public static float GetRandomFloat(float min, float max)
	{
		var randomNumber = new RandomNumberGenerator();
		randomNumber.Randomize();

		float randomFloat = randomNumber.RandfRange(min, max);
		
		return randomFloat;
	}
}
