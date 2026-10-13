using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class VictorySpawnerContainer : Node2D
{
	private List<GemSpawner> victorySpawners = new();

	public override void _Ready()
	{
		SubscribeToSignals();

		victorySpawners = GetChildren().OfType<GemSpawner>().ToList();
		DisableAllSpawners();
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
		InitializeSpawnerData();
		EnableAllSpawners();
	}

	private void InitializeSpawnerData()
	{
		foreach (GemSpawner spawner in victorySpawners)
		{
			spawner.InitializeVictoryVariables();
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
			GD.Print($"Spawner Spawn Time: {spawner.SpawnTime}");
		}
	}
}
