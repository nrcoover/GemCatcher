using Godot;

public partial class HeartRed : Node2D
{
	public override void _Ready()
	{
		SetHeartColor();
	}

	private void SetHeartColor()
	{
		Modulate = new Color(Constants.CustomColors.RedBright);
	}
}
