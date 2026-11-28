using System;
using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class MenuModal : Control
{
	[Export] TextureButton _closeButton;

	private readonly List<Control> _focusableControls = new();

	public event Action<MenuModal> Closed;

	public override void _Ready()
	{
		SubscribeToSignals();
		GetFocusableControls(this);
		Hide();
	}

  public override void _ExitTree()
  {
    UnsubscribeFromSignals();
  }

  private void SubscribeToSignals()
	{
		_closeButton.Pressed += OnCloseClicked;
	}

	private void UnsubscribeFromSignals()
	{
		_closeButton.Pressed -= OnCloseClicked;
	}

	private void OnCloseClicked()
	{
		Close();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!Visible)
		{
			return;
		}

		if (@event.IsActionPressed("ui_cancel"))
		{
			Close();
			GetViewport().SetInputAsHandled();
		}
	}

	public void Open()
	{
		Show();

		if (UIInputManager.Instance.CurrentMode == UIInputManager.InputMode.Gamepad)
		{
			GrabInitialFocus();
		}
		else
		{
			GetViewport().GuiReleaseFocus();
		}
	}

	public void Close()
	{
		if (!Visible)
		{
			return;
		}

		Hide();
		GetViewport().GuiReleaseFocus();
		Closed?.Invoke(this);
	}

	public void GrabInitialFocus()
	{
		foreach (Control control in _focusableControls)
		{
			if (CanReceiveFocus(control))
			{
				control.CallDeferred(Control.MethodName.GrabFocus);
				return;
			}
		}

		// Fallback if the modal has no other focusable controls.
		if (CanReceiveFocus(_closeButton))
		{
			_closeButton.CallDeferred(Control.MethodName.GrabFocus);
		}
	}

	private void GetFocusableControls(Node root)
	{
		foreach (Node child in root.GetChildren())
		{
			if (child is Control control
					&& control.FocusMode == Control.FocusModeEnum.All)
			{
				_focusableControls.Add(control);
			}

			if (child.GetChildCount() > 0)
			{
				GetFocusableControls(child);
			}
		}
	}

	private bool CanReceiveFocus(Control control)
	{
		if (control == null || !GodotObject.IsInstanceValid(control)
			|| (!control.IsVisibleInTree())
			|| (control.FocusMode != Control.FocusModeEnum.All)
			|| (control is BaseButton button && button.Disabled)
		)
		{
			return false;
		}
		
		return true;
	}
}
