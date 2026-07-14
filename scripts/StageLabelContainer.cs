using Godot;

public partial class StageLabelContainer : VBoxContainer
{
	[Export] private Label _stageLabel;
	[Export] private Timer _fadeOutTimer;
	private Tween _modulateTween;

	public override void _Ready()
	{
		UpdateUi();
		SubscribeToSignals();
	}

  public override void _ExitTree()
  {
		UnsubscribeFromSignals();
		KillAllTweens();
  }

  private void SubscribeToSignals()
  {
		_fadeOutTimer.Timeout += OnFadeOutTimerTimeout;
    SignalManager.Instance.AdvanceStage += OnAdvanceStage;
  }

  private void UnsubscribeFromSignals()
  {
		_fadeOutTimer.Timeout -= OnFadeOutTimerTimeout;
    SignalManager.Instance.AdvanceStage -= OnAdvanceStage;
  }

  private void OnAdvanceStage()
  {
    UpdateUi();
  }
	
  private void OnFadeOutTimerTimeout()
  {
		CreateFadeOutTween();
  }

  private void UpdateUi()
	{
		ShowStageLabel();
		_stageLabel.Text = $"STAGE {GameManager.Instance.CurrentStage}";
		_stageLabel.Modulate = new Color(Constants.CustomColors.White);
		_fadeOutTimer.Start();
	}

	private void ShowStageLabel()
	{
		_stageLabel.Visible = true;
	}

	private void KillAllTweens()
	{
		_modulateTween?.Kill();
	}

	private void CreateFadeOutTween()
	{
		_modulateTween = CreateTween();

		var tweenTime = 1.25f;

		_modulateTween.TweenProperty(
			_stageLabel,
			PropertyName.Modulate.ToString(),
			new Color(Constants.CustomColors.AplhaInvisible),
			tweenTime
		).SetTrans(Tween.TransitionType.Cubic)
		.SetEase(Tween.EaseType.Out);
	}
}
