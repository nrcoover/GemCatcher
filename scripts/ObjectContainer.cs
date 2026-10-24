using System.Collections.Generic;
using Godot;

public partial class ObjectContainer : Node
{
	[Export] private PackedScene _testScene;

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("test-pool"))
		{
			SignalManager.EmitSpawnPoolObject(
				new Vector2(
					GD.Randf() * 800,
					GD.Randf() * 300
				),
				_testScene,
				Helper.GetHexCodeFromColor(Helper.GetColorFromRainbow())
			);
		}
	}

	// TODO: Delete all above code after testing is complete!

	private readonly Dictionary<PackedScene, ScenePool> _pools = new();

	public override void _Ready()
	{
		SubscribeToSignals();
	}

	public override void _ExitTree()
	{
		UnsubscribeFromSignals();
	}

	private void SubscribeToSignals()
	{
		SignalManager.Instance.SpawnPoolObject += OnSpawnPoolObject;
	}

	private void UnsubscribeFromSignals()
	{
		SignalManager.Instance.SpawnPoolObject -= OnSpawnPoolObject;
	}

	private void OnSpawnPoolObject(Vector2 position, PackedScene scene, string colorHex = null)
	{
		CallDeferred(MethodName.SpawnDeffered, position, scene, colorHex);
	}

	private void SpawnDeffered(Vector2 position, PackedScene scene, string colorHex)
	{
		if (!_pools.TryGetValue(scene, out var pool))
		{
			int scenePoolSize = 5;
			pool = new ScenePool(scenePoolSize, scene, this, colorHex);
			_pools.Add(scene, pool);
		}

		pool.ActivateNext(position, colorHex);
	}
}
