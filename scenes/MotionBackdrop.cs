using Godot;
using System;

public partial class MotionBackdrop : Node2D
{
	[Export] Sprite2D _background1;
	[Export] Sprite2D _background2;
	[Export] Sprite2D _background3;
	[Export] Sprite2D _background4;

	public override void _Ready()
	{
		HideAllBackgrounds();
		SetVisibleBackground();
	}

	private void SetVisibleBackground()
	{
		var randomMin = 0;
		var randomMax = 4;

		var randonInteger = Helper.GetRandomInt(randomMin, randomMax) % randomMax;

		switch (randonInteger)
		{
			case 0:
				ShowBackground1();
				break;
			case 1:
				ShowBackground2();
				break;
			case 2:
				ShowBackground3();
				break;
			case 3:
				ShowBackground4();
				break;
		}
	}

	private void HideAllBackgrounds()
	{
		_background1.Hide();
		_background2.Hide();
		_background3.Hide();
		_background4.Hide();
	}

	private void ShowBackground1()
	{
		_background1.Show();
	}

	private void ShowBackground2()
	{
		_background2.Show();
	}

	private void ShowBackground3()
	{
		_background3.Show();
	}

	private void ShowBackground4()
	{
		_background4.Show();
	}
}
