using Godot;
using System.Collections.Generic;

public static class NodeExtensions
{
	public static List<Node> GetAllChildrenRecursive(this Node root)
	{
		var result = new List<Node>();
		AccumulateChildren(root, result);
		return result;
	}

	private static void AccumulateChildren(Node node, List<Node> list)
	{
		foreach (Node child in node.GetChildren())
		{
			list.Add(child);
			AccumulateChildren(child, list);
		}
	}

	public static List<T> GetFilteredChildrenRecursive<T>(this Node root) where T : Node
	{
		var result = new List<T>();
		AccumulateFilteredChildren(root, result);
		return result;
	}

	private static void AccumulateFilteredChildren<T>(Node node, List<T> list) where T : Node
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is T typedChild)
			{
				list.Add(typedChild);
			}
			AccumulateFilteredChildren(child, list);
		}
	}
}
