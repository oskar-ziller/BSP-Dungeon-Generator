#region

using System.Collections.Generic;
using UnityEngine;

#endregion

public class BinaryTree
{
	#region Constants and Fields

	public static float SplitMinRatio, SplitMaxRatio;
	public RectInt DungeonRoom;
	public BinaryTree LeftNode;
	public BinaryTree Parent;
	public BinaryTree RightNode;

	public RectInt RootNode;
	public SplitDirection SplitDir;
	public RectInt Tunnel;

	#endregion

	#region Constructors and Destructors

	public BinaryTree(RectInt rect)
	{
		RootNode = rect;
	}

	#endregion

	#region Public Methods

	/// <summary>
	///     Iteratively splits a tree into two parts and assigns the LeftNode and RightNode to the
	///     respective split containers.
	/// </summary>
	/// <param name="iterations">How many iterations to split</param>
	/// <param name="root_node">The container to do the split operations on</param>
	/// <param name="parent">Parent node (null for root)</param>
	/// <returns></returns>
	internal static BinaryTree SplitRecursively(int iterations, RectInt root_node, BinaryTree parent = null)
	{
		var node = new BinaryTree(root_node)
		{
			Parent = parent
		};

		if (iterations == 0)
		{
			return node;
		}

		node.Split();
		node.LeftNode = SplitRecursively(iterations - 1, node.LeftNode.RootNode, node);
		node.RightNode = SplitRecursively(iterations - 1, node.RightNode.RootNode, node);

		return node;
	}

	internal List<BinaryTree> GetAllChildren()
	{
		return Bfs(this);
	}

	internal List<BinaryTree> GetAllLeafs()
	{
		var to_return = new List<BinaryTree>();

		foreach (var c in GetAllChildren())
		{
			if (c.LeftNode == null && c.RightNode == null)
			{
				to_return.Add(c);
			}
		}

		return to_return;
	}

	internal List<BinaryTree> GetAllNonLeafs()
	{
		var to_return = new List<BinaryTree>();

		foreach (var c in GetAllChildren())
		{
			if (c.LeftNode != null && c.RightNode != null && c.Parent != null)
			{
				to_return.Add(c);
			}
		}

		return to_return;
	}

	internal List<RectInt> GetAllRooms()
	{
		var to_return = new List<RectInt>();
		var children = Bfs(this);

		foreach (var c in children)
		{
			if (!c.DungeonRoom.Equals(new RectInt()))
			{
				to_return.Add(c.DungeonRoom);
			}
		}

		return to_return;
	}

	internal List<RectInt> GetAllTunnels()
	{
		var to_return = new List<RectInt>();
		var children = Bfs(this);

		foreach (var c in children)
		{
			if (!c.Tunnel.Equals(new RectInt()))
			{
				to_return.Add(c.Tunnel);
			}
		}

		return to_return;
	}

	#endregion

	#region Private Methods

	private static List<BinaryTree> Bfs(BinaryTree tree)
	{
		var to_return = new List<BinaryTree>();
		var queue = new Queue<BinaryTree>();

		queue.Enqueue(tree);

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();

			to_return.Add(current);

			if (current.LeftNode != null)
			{
				queue.Enqueue(current.LeftNode);
			}

			if (current.RightNode != null)
			{
				queue.Enqueue(current.RightNode);
			}
		}

		return to_return;
	}

	private void Split()
	{
		var split_result = SplitContainer();
		LeftNode = new BinaryTree(split_result[0]);
		RightNode = new BinaryTree(split_result[1]);
	}

	private RectInt[] SplitContainer()
	{
		var c = RootNode;

		RectInt left_node, right_node;

		var vertical = c.width > c.height;

		
		if (vertical)
		{
			SplitDir = SplitDirection.Vertical;
			var w = Mathf.RoundToInt(c.width * Random.Range(SplitMinRatio, SplitMaxRatio));

			left_node = new RectInt(c.x, c.y, w, c.height);

			right_node = new RectInt(c.x + left_node.width, c.y, c.width - left_node.width, c.height);
		}
		else
		{
			SplitDir = SplitDirection.Horizontal;
			var h = Mathf.RoundToInt(c.height * Random.Range(SplitMinRatio, SplitMaxRatio));

			left_node = new RectInt(c.x, c.y, c.width, h);

			right_node = new RectInt(c.x, c.y + left_node.height, c.width, c.height - left_node.height);
		}

		return new[]
		{
			left_node, right_node
		};
	}

	#endregion
}