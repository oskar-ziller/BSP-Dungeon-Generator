using System.Collections.Generic;
using UnityEngine;

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
	public float SplitMinRatio { get; set; }
	public float SplitMaxRatio { get; set; }
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
		DungeonTree = BinaryTree.SplitIteratively(Iterations, StartingRect);
		CreateRooms();
		//CreateTunnels();
	}

	#endregion

	#region Private Methods

	/// <summary>
	///     Connects Left and Right children of a node.
	/// </summary>
	private void ConnectChildren(BinaryTree node)
	{
		var possible_tunnel_starts = new List<RectInt>();

		// If parent is split horizontal we want a vertical tunnel or vice versa
		var direction = node.SplitDir == SplitDirection.Vertical
			? TunnelDirection.Horizontal
			: TunnelDirection.Vertical;

		// iterate all rooms and save every point a tunnel can fit
		// takes into account the Padding value
		foreach (var room in node.LeftNode.GetAllRooms())
		{
			var room_start = 0;
			var room_end = 0;

			// Set roomStart after Padding units 
			// Subtract Padding and TunnelSize from actual room end

			// If we want a Vertical tunnel, we iterate from xMin to xMax
			if (direction == TunnelDirection.Vertical)
			{
				room_start = room.xMin + Padding;
				room_end = room.xMax - Padding * 2 - TunnelSize;
			}

			// If we want a Horizontal tunnel, we iterate from yMin to yMax
			if (direction == TunnelDirection.Horizontal)
			{
				room_start = room.yMin;
				room_end = room.yMax - Padding * 2 - TunnelSize;
			}

			for (var pos = room_start; pos <= room_end; pos++)
			{
				// For every location, create and save a tunnelStart with zero thickness
				//Debug.Log($"adding possible {direction} tunnel start at pos: {pos} room: {room}");

				if (direction == TunnelDirection.Vertical)
				{
					possible_tunnel_starts.Add(new RectInt(pos, room.yMax, TunnelSize + Padding * 2, 0));
				}

				if (direction == TunnelDirection.Horizontal)
				{
					possible_tunnel_starts.Add(new RectInt(room.xMax, pos, 0, TunnelSize + Padding * 2));
				}
			}
		}

		// Do the same for every tunnel
		foreach (var tunnel in node.LeftNode.GetAllTunnels())
		{
			var tunnel_start = 0;
			var tunnel_end = 0;

			if (direction == TunnelDirection.Vertical)
			{
				tunnel_start = tunnel.xMin;
				tunnel_end = tunnel.xMax - Padding * 2 - TunnelSize;
			}

			if (direction == TunnelDirection.Horizontal)
			{
				tunnel_start = tunnel.yMin;
				tunnel_end = tunnel.yMax - Padding * 2 - TunnelSize;
			}

			for (var pos = tunnel_start; pos <= tunnel_end; pos++)
			{
				//Debug.Log($"adding possible {direction} tunnel start at tunnel: " + tunnel.ToString());

				if (direction == TunnelDirection.Vertical)
				{
					possible_tunnel_starts.Add(new RectInt(pos, tunnel.yMax, TunnelSize + Padding * 2, 0));
				}

				if (direction == TunnelDirection.Horizontal)
				{
					possible_tunnel_starts.Add(new RectInt(tunnel.xMax, pos, 0, TunnelSize + Padding * 2));
				}
			}
		}

		var tunnels = new List<RectInt>();

		// iterate possibleTunnelStarts and see if they also fit at RightNode tunnels or rooms

		// rooms
		foreach (var st in possible_tunnel_starts)
		{
			foreach (var room in node.RightNode.GetAllRooms())
			{
				var point_to_check = new Vector2Int();
				var point_to_check2 = new Vector2Int();

				// Set pointToCheck to starting X of the possible tunnel and Y of the room we are checking
				// Set pointToCheck2 to the desired width
				if (direction == TunnelDirection.Vertical)
				{
					point_to_check = new Vector2Int(st.x, room.yMin);
					point_to_check2 = new Vector2Int(st.x + st.width, room.yMin);
				}

				if (direction == TunnelDirection.Horizontal)
				{
					point_to_check = new Vector2Int(room.xMin, st.y);
					point_to_check2 = new Vector2Int(room.xMin, st.y + st.height);
				}

				// if room contains both points, possibleTunnel fits the room
				if (room.Contains(point_to_check) && room.Contains(point_to_check2))
				{
					// resize the tunnel so it touches both rooms
					st.SetMinMax(st.min, point_to_check2);
					tunnels.Add(st);
				}
			}
		}

		// tunnels - do the same with rooms
		foreach (var st in possible_tunnel_starts)
		{
			foreach (var tunnel in node.RightNode.GetAllTunnels())
			{
				var point_to_check = new Vector2Int();
				var point_to_check2 = new Vector2Int();

				if (direction == TunnelDirection.Vertical)
				{
					point_to_check = new Vector2Int(st.x, tunnel.yMin);
					point_to_check2 = new Vector2Int(st.x + st.width, tunnel.yMin);
				}

				if (direction == TunnelDirection.Horizontal)
				{
					point_to_check = new Vector2Int(tunnel.xMin, st.y);
					point_to_check2 = new Vector2Int(tunnel.xMin, st.y + st.height);
				}

				if (tunnel.Contains(point_to_check) && tunnel.Contains(point_to_check2))
				{
					st.SetMinMax(st.min, point_to_check2);
					tunnels.Add(st);
				}
			}
		}

		var refined_tunnels = new List<RectInt>();

		// iterate all tunnels we found and see if they collide with other rooms or tunnels in other nodes
		foreach (var t in tunnels)
		{
			var collision = false;

			foreach (var c in DungeonTree.GetAllChildren())
			{
				if (c.Room.Overlaps(t))
				{
					collision = true;
					break;
				}

				if (c.Tunnel.Overlaps(t))
				{
					collision = true;
					break;
				}
			}

			if (!collision)
			{
				refined_tunnels.Add(t);
			}
		}

		var final = new List<RectInt>();

		// finally, remove the Padding from the tunnels
		for (var i = 0; i < refined_tunnels.Count; i++)
		{
			var r = refined_tunnels[i];

			if (direction == TunnelDirection.Vertical)
			{
				r.x += Padding;
				r.width -= Padding * 2;
			}

			if (direction == TunnelDirection.Horizontal)
			{
				r.y += Padding;
				r.height -= Padding * 2;
			}

			final.Add(r);
		}

		node.Tunnel = final[Random.Range(0, refined_tunnels.Count - 1)];
	}

	/// Create a room in every leaf that fits its container.
	private void CreateRooms()
	{
		foreach (var ch in DungeonTree.GetAllLeafs())
		{
			var c = ch.Container;
			var random_w = Mathf.Max(Mathf.RoundToInt(Random.Range(c.width * RoomMinRatio, c.width * RoomMaxRatio)), 1);
			var random_h = Mathf.Max(Mathf.RoundToInt(Random.Range(c.height * RoomMinRatio, c.height * RoomMaxRatio)), 1);

			var random_x = Random.Range(0, c.width - random_w);
			var random_y = Random.Range(0, c.height - random_h);

			ch.Room = new RectInt(c.x + random_x, c.y + random_y, random_w, random_h);
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

	#endregion
}