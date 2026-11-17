using Godot;
using System;

public partial class UIInputManager : Node
{
	public enum InputMode
	{
		MouseKeyboard,
		Gamepad
	}

	public static UIInputManager Instance { get; private set; }

	public event Action<InputMode> InputModeChanged;

	public InputMode CurrentMode { get; private set; } = InputMode.MouseKeyboard;

	private const float JOY_AXIS_THRESHOLD = 0.2f;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _ExitTree()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventJoypadButton joyButton:
				if (joyButton.Pressed)
				{
					SetInputMode(InputMode.Gamepad);
				}
				break;

			case InputEventJoypadMotion joyMotion:
				if (Mathf.Abs(joyMotion.AxisValue) > JOY_AXIS_THRESHOLD)
				{
					SetInputMode(InputMode.Gamepad);
				}
				break;

			case InputEventMouseMotion:
			case InputEventMouseButton:
				SetInputMode(InputMode.MouseKeyboard);
				break;

			case InputEventKey key:
				if (key.Pressed && !key.Echo)
				{
					SetInputMode(InputMode.MouseKeyboard);
				}
				break;
		}
	}

	private void SetInputMode(InputMode newMode)
	{
		if (CurrentMode == newMode)
		{
			return;
		}

		CurrentMode = newMode;

		Input.MouseMode = newMode == InputMode.Gamepad
			? Input.MouseModeEnum.Hidden
			: Input.MouseModeEnum.Visible;
		
		InputModeChanged?.Invoke(CurrentMode);
	}
}
