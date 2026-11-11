using Godot;

[GlobalClass]
public partial class HasPlayedGameResource : Resource
{
	private bool _hasPlayedGame;

	[Export]
	public bool HasPlayedGame
	{
		get
		{
			return _hasPlayedGame;
		}
		set
		{
			_hasPlayedGame = value;	
		}
	}
}