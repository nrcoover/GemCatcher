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

		var randomNumber = GetRandomInt(0, 9) % 9;

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
				selectedColorhex = Constants.CustomColors.BlueLightPastelle;
				break;

			case 5:
				selectedColorhex = Constants.CustomColors.BluePastelle;
				break;

			case 6:
				selectedColorhex = Constants.CustomColors.BlueDarkPastelle;
				break;

			case 7:
				selectedColorhex = Constants.CustomColors.PurplePastelle;
				break;

			case 8:
				selectedColorhex = Constants.CustomColors.PinkPastelle;
				break;
		}

		GD.Print(randomNumber, " ", selectedColorhex);

		return new Color(selectedColorhex);
	}

	public static int SetZIndexByColor(Color color)
	{
		int assignedIndex;

		GD.Print($"COLOR: {color}");

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
			assignedIndex = 1;
		}
		else if (color == orangePastelle)
		{
			assignedIndex = 3;
		}
		else if (color == yellowPastelle)
		{
			assignedIndex = 5;
		}
		else if (color == greenPastelle)
		{
			assignedIndex = 7;
		}
		else if (color == blueLightPastelle)
		{
			assignedIndex = 9;
		}
		else if (color == bluePastelle)
		{
			assignedIndex = 9;
		}
		else if (color == blueDarkPastelle)
		{
			assignedIndex = 9;
		}
		else if (color == purplePastelle)
		{
			assignedIndex = 11;
		}
		else if (color == pinkPastelle)
		{
			assignedIndex = 13;
		}
		else
		{
			assignedIndex = 15;
		}

		return assignedIndex;
	}
}
