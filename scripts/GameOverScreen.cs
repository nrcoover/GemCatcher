using System.Collections.Generic;
using Godot;

public partial class GameOverScreen : Control
{
	[Export] Panel _gameOverPanel;
	[Export] AnimationPlayer _animator;
	[Export] TextureButton _retryButton;
	[Export] TextureButton _exitButton;
	[Export] VBoxContainer _buttonsContainer;

	private readonly Dictionary<BaseButton, Control.MouseFilterEnum>
    _originalMouseFilters = new();

	public override void _Ready()
	{
		SubscribeToSignals();
		HandleStartupUI();

		_gameOverPanel.Visible = false;
		_buttonsContainer.Visible = false;
	}

	public override void _ExitTree()
	{
		UnsubscribeToSignals();
	}

  private void SubscribeToSignals()
  {
    SignalManager.Instance.ShowGameOverScreen += OnShowGameOverScreen;
		SignalManager.Instance.ShowMissionFailurePanel += OnShowMissionFailurePanel;
		SignalManager.Instance.ShowGameOverButtons += OnShowGameOverButtons;
		UIInputManager.Instance.InputModeChanged += OnInputModeChanged;
		_retryButton.Pressed += OnRetryButtonPressed;
		_exitButton.Pressed += OnExitButtonPressed;
  }

  private void UnsubscribeToSignals()
  {
    SignalManager.Instance.ShowGameOverScreen -= OnShowGameOverScreen;
		SignalManager.Instance.ShowMissionFailurePanel -= OnShowMissionFailurePanel;
		SignalManager.Instance.ShowGameOverButtons -= OnShowGameOverButtons;
		UIInputManager.Instance.InputModeChanged -= OnInputModeChanged;
  }

  private void OnShowMissionFailurePanel()
  {
    _gameOverPanel.Visible = true;
		_animator.Play("flash");
  }

  private void OnShowGameOverScreen()
  {
    Visible = true;
  }

	private void OnShowGameOverButtons()
	{
		_animator.Play("show-buttons");

		UIInputManager.Instance.SetInputContext(
			UIInputManager.InputContext.Menu
		);

		InitializeActiveMenuAsync();
	}

	private void OnInputModeChanged(UIInputManager.InputMode mode)
	{
		Control focusedControl = GetViewport().GuiGetFocusOwner();

    GD.Print(
			$"[GameOverScreen] INPUT MODE CHANGED\n" +
			$"  Mode: {mode}\n" +
			$"  Menu Interactive: {IsMenuInteractive()}\n" +
			$"  Retry Visible: {_retryButton.IsVisibleInTree()}\n" +
			$"  Retry Focus Mode: {_retryButton.FocusMode}\n" +
			$"  Retry Disabled: {_retryButton.Disabled}\n" +
			$"  Focus Owner: {(focusedControl != null ? focusedControl.Name : "None")}"
    );

		if (!IsMenuInteractive())
		{
			return;
		}

		bool isMouseKeyboard =
			mode == UIInputManager.InputMode.MouseKeyboard;

		UpdateButtonMouseInteraction(isMouseKeyboard);

		if (isMouseKeyboard)
		{
			ClearAllFocus();
			RefreshMenuMouseHover();
			return;
		}

		if (!IsAnyControlFocused() && CanReceiveFocus(_retryButton))
		{
			_retryButton.GrabFocus();
		}
	}
	
  private void OnRetryButtonPressed()
  {
		SignalManager.Instance.EmitResetGame();
    LevelManager.Instance.LoadGame();
  }

  private void OnExitButtonPressed()
  {
		SignalManager.Instance.EmitResetGame();
    LevelManager.Instance.LoadMainMenu();
  }



#region Asynchronous Tasks

	private async void InitializeActiveMenuAsync()
	{
		while (IsInsideTree() && !_buttonsContainer.IsVisibleInTree())
		{
			await ToSignal(
				GetTree(),
				SceneTree.SignalName.ProcessFrame
			);
		}

		if (!IsInsideTree() || !IsMenuInteractive())
		{
			return;
		}

		OnInputModeChanged(UIInputManager.Instance.CurrentMode);
	}

#endregion



#region UI Handling

	private void HandleStartupUI()
	{
		CacheOriginalMouseFilters(this);
	}

	private void UpdateButtonMouseInteraction(bool allowMouseInteraction)
	{
		foreach (var entry in _originalMouseFilters)
		{
			if (!GodotObject.IsInstanceValid(entry.Key))
			{
				continue;
			}

			entry.Key.MouseFilter = allowMouseInteraction
				? entry.Value
				: Control.MouseFilterEnum.Ignore;
		}
	}

	private void ClearAllFocus()
	{
		GetViewport().GuiReleaseFocus();
	}

	private bool IsAnyControlFocused()
	{
		Control focusedControl = GetViewport().GuiGetFocusOwner();

    return focusedControl != null
			&& IsAncestorOf(focusedControl)
			&& CanReceiveFocus(focusedControl);
	}

	private bool CanReceiveFocus(Control control)
	{
		if (control == null || !GodotObject.IsInstanceValid(control)
			|| (!control.IsVisibleInTree())
			|| (control.FocusMode == Control.FocusModeEnum.None)
			|| (control is BaseButton button && button.Disabled)
		)
		{
			return false;
		}

		return true;
	}

	private void RefreshMenuMouseHover()
	{
		foreach (BaseButton button in _originalMouseFilters.Keys)
		{
			if (button is GemButton gemButton
				&& GodotObject.IsInstanceValid(gemButton)
				&& gemButton.IsVisibleInTree())
			{
				gemButton.RefreshMouseHoverState();
			}
		}
	}

	private void CacheOriginalMouseFilters(Node node)
	{
		if (node is BaseButton button)
		{
			_originalMouseFilters.TryAdd(button, button.MouseFilter);
		}

		foreach (Node child in node.GetChildren())
		{
			CacheOriginalMouseFilters(child);
		}
	}

	private bool IsMenuInteractive()
	{
		return IsVisibleInTree()
			&& _buttonsContainer.IsVisibleInTree();
	}

#endregion

}
