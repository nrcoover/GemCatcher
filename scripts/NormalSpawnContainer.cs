using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class NormalSpawnContainer : Node2D
{
	private List<GemSpawner> _normalSpawners = new();
	private List<Gem> _gems = new();

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
		GetAllActiveGems();
		CallDeferred(MethodName.DisableAllSpawners);
		CallDeferred(MethodName.EnableAllGems);
		// TODO: Handle frozen gems issue. (BUG)
	}

	private void InitializeSpawnerData()
	{
		foreach (GemSpawner spawner in _normalSpawners)
		{
			var minTime = 0.1f;
			var maxTime = 0.75f;

			spawner.SpawnTime = Helper.GetRandomFloat(minTime, maxTime);
		}
	}

	private void DisableAllSpawners()
	{
		foreach (GemSpawner spawner in _normalSpawners)
		{
			spawner.ProcessMode = ProcessModeEnum.Disabled;
		}
	}

	private void EnableAllSpawners()
	{
		foreach (GemSpawner spawner in _normalSpawners)
		{
			spawner.ProcessMode = ProcessModeEnum.Inherit;
		}
	}

	// Enables regular gems on victory sequence so they explode
	private void EnableAllGems()
	{
		foreach (Gem gem in _gems)
		{
			gem.ProcessMode = ProcessModeEnum.Always;
		}
	}

	private void GetAllActiveSpawners()
	{
		_normalSpawners = GetChildren().OfType<GemSpawner>().ToList();
	}

	private void GetAllActiveGems()
	{
		_gems = this.GetFilteredChildrenRecursive<Gem>();
		GD.PrintErr($"Active Gem Count: {_gems.Count}");
	}
}
