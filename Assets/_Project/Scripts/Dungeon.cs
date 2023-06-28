#region

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

public class Dungeon
{
	#region Constants and Fields

	public BinaryTree DungeonTree;

	#endregion

	#region Properties

	public int Iterations { get; set; }
	public int Padding { get; set; }
	public float RoomMaxRatio { get; set; }
	public float RoomMinRatio { get; set; }
	public float SplitMaxRatio { get; set; }
	public float SplitMinRatio { get; set; }
	public RectInt StartingRect { get; set; }
	public int TotalSize { get; set; }
	public int TunnelSize { get; set; }

	#endregion

	#region Public Methods

	/// <summary>
	///     Split the DungeonTree and create rooms and tunnels
	/// </summary>
	public void Generate()
	{
		BinaryTree.SplitMinRatio = SplitMinRatio;
		BinaryTree.SplitMaxRatio = SplitMaxRatio;
		DungeonTree = BinaryTree.SplitRecursively(Iterations, StartingRect);
		CreateRooms();
		CreateTunnels();
	}

	#endregion

	#region Private Methods

	private void AdjustPadding(List<RectInt> tunnels, TunnelDirection direction)
	{
		for (var i = 0; i < tunnels.Count; i++)
		{
			var rect = tunnels[i];

			switch (direction)
			{
				case TunnelDirection.Vertical:
					rect.x += Padding;
					rect.width -= Padding;
					break;
				case TunnelDirection.Horizontal:
					rect.y += Padding;
					rect.height -= Padding;
					break;
			}

			tunnels[i] = rect;
		}
	}

	private void AppendPossibleTunnelStarts(
		List<RectInt> result,
		IEnumerable<RectInt> elements,
		TunnelDirection direction)
	{
		foreach (var element in elements)
		{
			var start = GetStart(direction, element);
			var end = GetEnd(direction, element);

			for (var pos = start; pos <= end; pos++)
			{
				switch (direction)
				{
					case TunnelDirection.Vertical:
						result.Add(new RectInt(pos, element.yMax, TunnelSize + Padding, 0));
						break;
					case TunnelDirection.Horizontal:
						result.Add(new RectInt(element.xMax, pos, 0, TunnelSize + Padding));
						break;
				}
			}
		}
	}

	private void AppendTunnels(
		List<RectInt> result,
		List<RectInt> tunnel_starts,
		IEnumerable<RectInt> areas,
		TunnelDirection direction)
	{
		foreach (var ts in tunnel_starts)
		{
			foreach (var area in areas)
			{
				var point_to_check = GetPoint1(direction, ts, area);
				var point_to_check2 = GetPoint2(direction, ts, area);

				if (!area.Contains(point_to_check) || !area.Contains(point_to_check2))
				{
					continue;
				}

				ts.SetMinMax(ts.min, point_to_check2);
				result.Add(ts);
			}
		}
	}

	private List<RectInt> CalculatePossibleTunnelStarts(BinaryTree node)
	{
		var possible_tunnel_starts = new List<RectInt>();
		var direction = GetTunnelDirection(node);
		AppendPossibleTunnelStarts(possible_tunnel_starts, node.LeftNode.GetAllRooms(), direction);
		AppendPossibleTunnelStarts(possible_tunnel_starts, node.LeftNode.GetAllTunnels(), direction);

		return possible_tunnel_starts;
	}

	private List<RectInt> CalculateTunnels(BinaryTree node, List<RectInt> tunnel_starts)
	{
		var tunnels = new List<RectInt>();
		var direction = GetTunnelDirection(node);

		AppendTunnels(tunnels, tunnel_starts, node.RightNode.GetAllRooms(), direction);
		AppendTunnels(tunnels, tunnel_starts, node.RightNode.GetAllTunnels(), direction);

		return tunnels;
	}

	/// <summary>
	///     Connects Left and Right children of a node.
	/// </summary>
	private void ConnectChildren(BinaryTree node)
	{
		var possible_tunnel_starts = CalculatePossibleTunnelStarts(node);
		var tunnels = CalculateTunnels(node, possible_tunnel_starts);
		var refined_tunnels = RefineTunnels(tunnels);

		AdjustPadding(refined_tunnels, GetTunnelDirection(node));

		if (refined_tunnels.Any())
		{
			node.Tunnel = refined_tunnels[Random.Range(0, refined_tunnels.Count - 1)];
		}
	}

	/// Create a room in every leaf that fits its container.
	private void CreateRooms()
	{
		foreach (var container in DungeonTree.GetAllLeafs())
		{
			var root = container.RootNode;

			// random size for the room
			var random_w =
				Mathf.Max(Mathf.RoundToInt(Random.Range(root.width * RoomMinRatio, root.width * RoomMaxRatio)), 1);

			var random_h =
				Mathf.Max(Mathf.RoundToInt(Random.Range(root.height * RoomMinRatio, root.height * RoomMaxRatio)), 1);

			// random position for the room
			var random_x = Random.Range(0, root.width - random_w);
			var random_y = Random.Range(0, root.height - random_h);

			container.DungeonRoom = new RectInt(root.x + random_x, root.y + random_y, random_w, random_h);
		}
	}

	// Create tunnels by 1st connecting leafs with eachother and then
	// connecting every node with each other
	private void CreateTunnels()
	{
		// connect leafs
		foreach (var ch in DungeonTree.GetAllLeafs())
		{
			var parent = ch.Parent;

			if (parent.Tunnel.Equals(new RectInt()))
			{
				ConnectChildren(parent);
			}
		}

		// connect nodes
		foreach (var ch in DungeonTree.GetAllNonLeafs())
		{
			var parent = ch.Parent;

			if (parent.Tunnel.Equals(new RectInt()))
			{
				ConnectChildren(parent);
			}
		}
	}

	private int GetEnd(TunnelDirection direction, RectInt area)
	{
		return direction switch
		{
			TunnelDirection.Vertical => area.xMax - Padding - TunnelSize,
			TunnelDirection.Horizontal => area.yMax - Padding - TunnelSize,
			_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
		};
	}

	private Vector2Int GetPoint1(TunnelDirection direction, RectInt tunnel_start, RectInt area)
	{
		return direction switch
		{
			TunnelDirection.Vertical => new Vector2Int(tunnel_start.x, area.yMin),
			TunnelDirection.Horizontal => new Vector2Int(area.xMin, tunnel_start.y),
			_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
		};
	}

	private Vector2Int GetPoint2(TunnelDirection direction, RectInt tunnel_start, RectInt area)
	{
		return direction switch
		{
			TunnelDirection.Vertical => new Vector2Int(tunnel_start.x + tunnel_start.width, area.yMin),
			TunnelDirection.Horizontal => new Vector2Int(area.xMin, tunnel_start.y + tunnel_start.height),
			_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
		};
	}

	private int GetStart(TunnelDirection direction, RectInt area)
	{
		return direction switch
		{
			TunnelDirection.Vertical => area.xMin + Padding,
			TunnelDirection.Horizontal => area.yMin,
			_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
		};
	}

	private TunnelDirection GetTunnelDirection(BinaryTree node)
	{
		return node.SplitDir == SplitDirection.Vertical ? TunnelDirection.Horizontal : TunnelDirection.Vertical;
	}

	private bool IsTunnelCollisionFree(RectInt tunnel)
	{
		var collision = false;

		foreach (var c in DungeonTree.GetAllChildren())
		{
			if (c.DungeonRoom.Overlaps(tunnel) || c.Tunnel.Overlaps(tunnel))
			{
				collision = true;
				break;
			}
		}

		return !collision;
	}

	private List<RectInt> RefineTunnels(List<RectInt> tunnels)
	{
		var refined_tunnels = tunnels.Where(t => IsTunnelCollisionFree(t)).ToList();
		return refined_tunnels;
	}

	#endregion
}