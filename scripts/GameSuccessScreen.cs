using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class GameSuccessScreen : Control
{
	[Export] Panel _gameSuccessPanel;
	[Export] AnimationPlayer _animator;
	[Export] TextureButton _replayButton;
	[Export] TextureButton _exitButton;
	[Export] VBoxContainer _buttonsContainer;

	private readonly Dictionary<BaseButton, Control.MouseFilterEnum>
    _originalMouseFilters = new();

	public override void _Ready()
	{
		SubscribeToSignals();
		HandleStartupUI();

		_buttonsContainer.Visible = false;
		_gameSuccessPanel.Visible = false;
		_gameSuccessPanel.Modulate = new Color(Constants.CustomColors.AplhaInvisible);
		Modulate = new Color(Constants.CustomColors.AplhaInvisible);
	}

	public override void _ExitTree()
	{
		UnsubscribeToSignals();
	}

  private void SubscribeToSignals()
  {
		SignalManager.Instance.InitiateVictorySequence += OnInitiateVictorySequenceAsync;
		UIInputManager.Instance.InputModeChanged += OnInputModeChanged;
		_replayButton.Pressed += OnRetryButtonPressed;
		_exitButton.Pressed += OnExitButtonPressed;
  }

  private void UnsubscribeToSignals()
  {
		SignalManager.Instance.InitiateVictorySequence -= OnInitiateVictorySequenceAsync;
		UIInputManager.Instance.InputModeChanged -= OnInputModeChanged;
  }

	private async void OnInitiateVictorySequenceAsync()
	{
		ShowGameSuccessScreen();
		
		await CreateTimerAsync(3.5f); // Above animation is 4 seconds; this leaves a transparency by ending the animation early (accidental, but created a desirable side-effect).

		ShowMissionSuccessPanel();

		await CreateTimerAsync(4.0f);

		ShowGameSuccessButtons();
	}

	private void OnInputModeChanged(UIInputManager.InputMode mode)
	{
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

		if (!IsAnyControlFocused() && CanReceiveFocus(_replayButton))
		{
			_replayButton.GrabFocus();
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

	private void ShowMissionSuccessPanel()
  {
    _gameSuccessPanel.Visible = true;
		PlayShowSuccessPanelAnimation();
  }

  private void ShowGameSuccessScreen()
  {
    Visible = true;
		PlayScreenFadeInAnimation();
  }

	private void ShowGameSuccessButtons()
	{
		PlayShowButtonsAnimation();
	}


	private void PlayScreenFadeInAnimation()
	{
		_animator.Play(Constants.Animations.GameSuccessScreen.FadeIn);
	}
	
	private void PlayShowSuccessPanelAnimation()
	{
		_animator.Play(Constants.Animations.GameSuccessScreen.ShowSuccessPanel);
	}

	private void PlayShowButtonsAnimation()
	{
		_animator.Play(Constants.Animations.GameSuccessScreen.ShowButtons);

		UIInputManager.Instance.SetInputContext(
			UIInputManager.InputContext.Menu
		);

		InitializeActiveMenuAsync();
	}

	#region Asynchronous Tasks

	private async Task CreateTimerAsync(float timeInSeconds)
	{
		var tree = GetTree();

		if (tree == null)
		{
			return;
		}

		await ToSignal(
			tree.CreateTimer(timeInSeconds),
			SceneTreeTimer.SignalName.Timeout
		);
	}

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
