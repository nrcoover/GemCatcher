using System.Collections.Generic;
using System.Linq;
using Godot;

[GlobalClass]
public partial class MenuModal : Control
{
	[Export] TextureButton _closeButton;

	private readonly List<GemButton> _buttons = new();

	public override void _Ready()
	{
		SubscribeToSignals();
		GetButtons();
		CloseModal();
	}

  public override void _UnhandledInput(InputEvent @event)
  {
		if (!Visible)
		{
			return;
		}

		if (@event.IsActionPressed("ui_up")
			|| @event.IsActionPressed("ui_down")
			|| @event.IsActionPressed("ui_right")
			|| @event.IsActionPressed("ui_left")
			|| @event.IsActionPressed("ui_focus_prev")
			|| @event.IsActionPressed("ui_focus_next")
		)
		{
			_closeButton.CallDeferred(MethodName.GrabFocus);
		}
  }

  public override void _ExitTree()
  {
    UnsubscribeFromSignals();
  }

  private void SubscribeToSignals()
	{
		SignalManager.Instance.ModalOpened += OnModalOpened;
		_closeButton.Pressed += OnCloseClicked;
	}

	private void UnsubscribeFromSignals()
	{
		SignalManager.Instance.ModalOpened -= OnModalOpened;
		_closeButton.Pressed -= OnCloseClicked;
	}

	private void OnModalOpened()
	{
		if (Visible)
		{
			_buttons.FirstOrDefault().CallDeferred(Control.MethodName.GrabFocus);
		}
	}

	private void OnCloseClicked()
	{
		CloseModal();
		SignalManager.Instance.EmitEnableMainMenuFocus();
	}

	private void CloseModal()
	{
		Visible = false;
	}

	private void GetButtons()
	{
		foreach (Node child in this.GetChildren())
		{
			if (child is GemButton button)
			{
				_buttons.Add(button);
			}
		}
	}
}
