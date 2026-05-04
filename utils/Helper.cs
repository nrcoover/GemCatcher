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

	public static Color GetColorFromRainbow()
	{
		string selectedColorhex = Constants.CustomColors.RedBright;

		var randomNumber = GetRandomInt(0, 7) % 7;

		switch (randomNumber)
		{
			case 0:
				selectedColorhex = Constants.CustomColors.RedBright;
				break;
			
			case 1:
				selectedColorhex = Constants.CustomColors.OrangeBright;
				break;

			case 2:
				selectedColorhex = Constants.CustomColors.YellowBright;
				break;

			case 3:
				selectedColorhex = Constants.CustomColors.GreenBright;
				break;

			case 4:
				selectedColorhex = Constants.CustomColors.BlueBright;
				break;

			case 5:
				selectedColorhex = Constants.CustomColors.PurpleBright;
				break;

			case 6:
				selectedColorhex = Constants.CustomColors.PinkBright;
				break;
		}

		GD.Print(randomNumber, " ", selectedColorhex);

		return new Color(selectedColorhex);
	}
}
