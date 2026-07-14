using Godot;

public partial class MenuModal : Control
{
	[Export] TextureButton _closeButton;

	public override void _Ready()
	{
		SubscribeToSignals();
		CloseModal();
	}

	public override void _ExitTree()
	{
		_closeButton.Pressed -= OnCloseClicked;
	}

  private void SubscribeToSignals()
	{
		_closeButton.Pressed += OnCloseClicked;
	}

	private void OnCloseClicked()
	{
		CloseModal();
	}

	private void CloseModal()
	{
		Visible = false;
	}
}
