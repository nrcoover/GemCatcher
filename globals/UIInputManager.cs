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

	private bool _ignoreMouseMotion;

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
				if (CurrentContext != InputContext.Menu)
				{
					break;
				}

				if (_ignoreMouseMotion)
				{
					_ignoreMouseMotion = false;
					break;
				}

				if (mouseMotion.Relative.LengthSquared() > 0.0f)
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
		if (CurrentContext == context)
		{
			return;
		}

		InputContext previousContext = CurrentContext;

		// Ignore the first mouse motion event when transitioning
		// from captured gameplay into a menu.
		if (previousContext == InputContext.Gameplay
			&& context == InputContext.Menu)
		{
			_ignoreMouseMotion = true;
		}

		Input.MouseModeEnum previousMouseMode = Input.MouseMode;

		CurrentContext = context;

		UpdateCursorVisibility();

		GD.Print(
			$"[UIInputManager] INPUT CONTEXT CHANGED\n" +
			$"  Previous: {previousContext}\n" +
			$"  New: {CurrentContext}\n" +
			$"  Active Input: {CurrentMode}\n" +
			$"  Mouse Mode: {previousMouseMode} -> {Input.MouseMode}\n" +
			$"  Ignore Mouse Motion: {_ignoreMouseMotion}"
		);
	}
}
