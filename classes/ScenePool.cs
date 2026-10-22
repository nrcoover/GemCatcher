using System.Collections.Generic;
using System.Linq;
using GemCatcher.utils;
using Godot;

public class ScenePool
{
  private readonly List<Node2D> _items = new();
  private readonly PackedScene _packedScene;
  private readonly Node _container;

  int ActiveCount => _items.Count(n => n.Visible);

  public ScenePool(int initialCount, PackedScene scene, Node container)
  {
    _container = container;
    _packedScene = scene;

    Log($"Creating pool with {initialCount} items");

    for (int i = 0; i < initialCount; i++)
    {
      AddNewItem();
    }
  }

  public void ActivateNext(Vector2 position) {
    Log($"ActivateNext Active: {ActiveCount}/{_items.Count}");

    foreach (var node in _items)
    {
      if (!node.Visible)
      {
        Log($"Reusing instance {node.Name}");
        node.Show();
        node.GlobalPosition = position;
        ((IPoolItem)node).Activate();
        return;
      }
    }

    Log($"Nothing free, extending pool");
    Node2D newItem = AddNewItem();
    newItem.Show();
    newItem.GlobalPosition = position;
    ((IPoolItem)newItem).Activate();
  }

  private Node2D AddNewItem()
  {
    Node2D newItem = _packedScene.Instantiate<Node2D>();

    if (newItem is not IPoolItem)
    {
      GD.PrintErr($"{newItem.Name} does not implement IPoolItem");
    }

    newItem.Hide();
    _container.AddChild(newItem);
    _items.Add(newItem);

    Log($"New instance {newItem.Name} total: {_items.Count}");

    return newItem;
  }

  private void Log(string message) 
  {
    if (CustomDebug.IsDebugging)
    {
      GD.Print($"[ScenePool: {_packedScene.ResourcePath}] {message}");
    }
  }
}
