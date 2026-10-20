using Godot;

[GlobalClass]
public partial class InfiniteModeResource : Resource
{
	private bool _isInfiniteModeUnlocked;

	[Export]
	public bool IsInfiniteModeUnlocked
	{
		get
		{
			return _isInfiniteModeUnlocked;
		}
		set
		{
			_isInfiniteModeUnlocked = value;	
		}
	}
}