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

  public ScenePool(int initialCount, PackedScene scene, Node container, string hexColor)
  {
    _container = container;
    _packedScene = scene;

    Log($"Creating pool with {initialCount} items");

    for (int i = 0; i < initialCount; i++)
    {
      AddNewItem(hexColor);
    }
  }

  public void ActivateNext(Vector2 position, string colorHex) {
    Log($"ActivateNext Active: {ActiveCount}/{_items.Count}");

    foreach (var node in _items)
    {
      if (!node.Visible)
      {
        Log($"Reusing instance {node.Name}");
        
        if (!string.IsNullOrEmpty(colorHex))
        {
          node.Modulate = Helper.GetColorFromHexCode(colorHex);
        }
        
        node.Show();
        node.GlobalPosition = position;
        ((IPoolItem)node).Activate();
        return;
      }
    }

    Log($"Nothing free, extending pool");
    Node2D newItem = AddNewItem(colorHex);
    newItem.Show();
    newItem.GlobalPosition = position;
    ((IPoolItem)newItem).Activate();
  }

  private Node2D AddNewItem(string hexColor)
  {
    Node2D newItem = _packedScene.Instantiate<Node2D>();

    if (!string.IsNullOrEmpty(hexColor))
    {
      newItem.Modulate = Helper.GetColorFromHexCode(hexColor);
    }

    if (newItem is not IPoolItem)
    {
      GD.PrintErr($"{newItem.Name} does not implement IPoolItem");
    }

    newItem.Hide();
    _container.AddChild(newItem);
    _items.Add(newItem);

    Log($"New instance {newItem.Name} total: {_items.Count} / color: {newItem.Modulate}");

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
