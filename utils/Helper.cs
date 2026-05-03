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
		string selectedColorhex = Constants.CustomColors.RedPastelle;

		var randomNumber = GetRandomInt(0, 7) % 7;

		switch (randomNumber)
		{
			case 0:
				selectedColorhex = Constants.CustomColors.RedPastelle;
				break;
			
			case 1:
				selectedColorhex = Constants.CustomColors.OrangePastelle;
				break;

			case 2:
				selectedColorhex = Constants.CustomColors.YellowPastelle;
				break;

			case 3:
				selectedColorhex = Constants.CustomColors.GreenPastelle;
				break;

			case 4:
				selectedColorhex = Constants.CustomColors.BluePastelle;
				break;

			case 5:
				selectedColorhex = Constants.CustomColors.PurplePastelle;
				break;

			case 6:
				selectedColorhex = Constants.CustomColors.PinkPastelle;
				break;
		}

		GD.Print(randomNumber, " ", selectedColorhex);

		return new Color(selectedColorhex);
	}
}
