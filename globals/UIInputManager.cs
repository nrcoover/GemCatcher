using Godot;
using System;

public partial class UIInputManager : Node
{
	public enum InputMode
	{
		MouseKeyboard,
		Gamepad
	}

	public enum InputContext
	{
		Menu,
		Gameplay
	}

	public static UIInputManager Instance { get; private set; }

	public event Action<InputMode> InputModeChanged;

	public InputMode CurrentMode { get; private set; } = InputMode.MouseKeyboard;

	public InputContext CurrentContext { get; private set; } = InputContext.Menu;

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

			case InputEventMouseMotion mouseMotion:
				if (CurrentContext == InputContext.Menu
					&& mouseMotion.Relative.LengthSquared() > 0.0f)
				{
					SetInputMode(InputMode.MouseKeyboard);
				}
				break;

			case InputEventMouseButton mouseButton:
				if (CurrentContext == InputContext.Menu && mouseButton.Pressed)
				{
					SetInputMode(InputMode.MouseKeyboard);
				}
				break;

			case InputEventKey key:
				if (key.Pressed && !key.Echo)
				{
					SetInputMode(InputMode.MouseKeyboard);
				}
				break;
		}
	}

	private void UpdateCursorVisibility()
	{
		if (CurrentContext == InputContext.Gameplay)
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
			return;
		}

		Input.MouseMode = CurrentMode == InputMode.Gamepad
			? Input.MouseModeEnum.Hidden
			: Input.MouseModeEnum.Visible;
	}

	private void SetInputMode(InputMode newMode)
	{
		if (CurrentMode == newMode)
		{
			return;
		}

		GD.Print($"INPUT MODE CHANGED: {CurrentMode} -> {newMode}");

		CurrentMode = newMode;

		UpdateCursorVisibility();

		InputModeChanged?.Invoke(newMode);
	}

	public void SetInputContext(InputContext context)
	{
		CurrentContext = context;
		UpdateCursorVisibility();
	}
}
