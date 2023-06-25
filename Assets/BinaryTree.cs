#region

using System.Collections.Generic;
using UnityEngine;

#endregion

public class BinaryTree
{
	#region Constants and Fields

	public RectInt Container;
	public BinaryTree LeftNode;
	public BinaryTree Parent;
	public BinaryTree RightNode;
	public RectInt Room;
	public SplitDirection SplitDir;
	public RectInt Tunnel;

	public static float SplitMinRatio, SplitMaxRatio;
	
	#endregion

	#region Constructors and Destructors

	public BinaryTree(RectInt rect)
	{
		Container = rect;
	}

	#endregion

	#region Public Methods

	/// <summary>
	///     Splits a tree into two parts iteratively. Assigns LeftNode and RightNode to split containers.
	/// </summary>
	/// <param name="iterations">How many iterations to split</param>
	/// <param name="container">The container to do the split operations on</param>
	/// <param name="parent">Parent node (null for root)</param>
	/// <returns></returns>
	internal static BinaryTree SplitIteratively(int iterations, RectInt container, BinaryTree parent = null)
	{
		var node = new BinaryTree(container)
		{
			Parent = parent
		};

		if (iterations == 0)
		{
			return node;
		}

		node.Split();
		node.LeftNode = SplitIteratively(iterations - 1, node.LeftNode.Container, node);
		node.RightNode = SplitIteratively(iterations - 1, node.RightNode.Container, node);

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
			if (!c.Room.Equals(new RectInt()))
			{
				to_return.Add(c.Room);
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
		var c = Container;

		RectInt c1, c2;

		var vertical = c.width > c.height;

		if (vertical)
		{
			SplitDir = SplitDirection.Vertical;
			var w = Mathf.RoundToInt(c.width * Random.Range(SplitMinRatio, SplitMaxRatio));
			
			c1 = new RectInt(c.x, c.y,w, c.height);
			c2 = new RectInt(c.x + c1.width, c.y, c.width - c1.width, c.height);
		}
		else
		{
			SplitDir = SplitDirection.Horizontal;
			var h = Mathf.RoundToInt(c.height * Random.Range(SplitMinRatio, SplitMaxRatio));
			
			c1 = new RectInt(c.x, c.y, c.width, h);
			c2 = new RectInt(c.x, c.y + c1.height, c.width, c.height - c1.height);
		}

		return new[]
		{
			c1, c2
		};
	}

	#endregion
}