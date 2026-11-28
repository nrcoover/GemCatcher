using Godot;

[GlobalClass]
public partial class GemLinkButton : GemButton
{
	[Export] public string Url { get; set; } = "";
	public override void _Ready()
	{
		base._Ready();
	}

	protected override void SubscribeToSignals()
	{
		base.SubscribeToSignals();

		Pressed += OnButtonPressed;
	}

	private void OnButtonPressed()
	{
		if (!string.IsNullOrEmpty(Url))
		{
			OS.ShellOpen(Url);
		}
	}
}
