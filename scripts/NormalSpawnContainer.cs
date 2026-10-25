using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class NormalSpawnContainer : Node2D
{
	private List<GemSpawner> victorySpawners = new();

	public override void _Ready()
	{
		SubscribeToSignals();

		GetAllActiveSpawners();
		EnableAllSpawners();
	}

  public override void _ExitTree()
	{
		UnsubscribeFromSignals();
	}

	private void SubscribeToSignals()
	{
		SignalManager.Instance.GameVictory += OnGameVictory;
	}

	private void UnsubscribeFromSignals()
	{
		SignalManager.Instance.GameVictory -= OnGameVictory;
	}

	private void OnGameVictory()
	{
		GetAllActiveSpawners();
		CallDeferred(MethodName.DisableAllSpawners);
		// TODO: Handle frozen gems issue. (BUG)
	}

	private void InitializeSpawnerData()
	{
		foreach (GemSpawner spawner in victorySpawners)
		{
			var minTime = 0.1f;
			var maxTime = 0.75f;

			spawner.SpawnTime = Helper.GetRandomFloat(minTime, maxTime);
		}
	}

	private void DisableAllSpawners()
	{
		foreach (GemSpawner spawner in victorySpawners)
		{
			spawner.ProcessMode = ProcessModeEnum.Disabled;
		}
	}

	private void EnableAllSpawners()
	{
		foreach (GemSpawner spawner in victorySpawners)
		{
			spawner.ProcessMode = ProcessModeEnum.Inherit;
		}
	}

	private void GetAllActiveSpawners()
	{
		victorySpawners = GetChildren().OfType<GemSpawner>().ToList();
	}
}
