#region

using System.Reflection;
using UnityEditor;
using UnityEngine;

#endregion

public enum TunnelDirection
{
	Horizontal,
	Vertical
}

public enum SplitDirection
{
	Horizontal,
	Vertical
}

public class DungeonGenerator : MonoBehaviour
{
	#region Serialized Fields

	[Header("Dungeon Settings")]
	[SerializeField] [Range(1, 400)] private int totalSize = 12;
	[SerializeField] [Range(1, 10)] private int iterations = 1;
	[SerializeField] [Range(1, 10)] private int tunnelSize = 3;
	[SerializeField] [Range(0.1f, 1f)] private float roomMinRatio = 0.5f;
	[SerializeField] [Range(0.1f, 1f)] private float roomMaxRatio = 1f;
	[SerializeField] [Range(0, 5)] private int padding = 1;
	
	[SerializeField] [Range(0, 1)] private float splitMinRatio = 0.4f;
	[SerializeField] [Range(0, 1)] private float splitMaxRatio = 0.6f;

	[Header("Other")]
	[SerializeField] private bool drawDebugLines;
	[SerializeField] private bool clearConsoleOnGenerate;

	#endregion

	#region Constants and Fields

	public Dungeon Dungeon;

	#endregion

	#region Unity Methods

	private void OnDrawGizmos()
	{
		if (drawDebugLines)
		{
			DebugDraw();
		}
	}

	#endregion

	#region Public Methods

	public void Create()
	{
		if (clearConsoleOnGenerate)
		{
			ClearConsole();
		}
		
		var starting_rect = new RectInt(0, 0, totalSize, totalSize);

		var my_dungeon = new Dungeon
		{
			TunnelSize = tunnelSize,
			TotalSize = totalSize,
			RoomMinRatio = roomMinRatio,
			RoomMaxRatio = roomMaxRatio,
			SplitMinRatio = splitMinRatio,
			SplitMaxRatio = splitMaxRatio,
			StartingRect = starting_rect,
			Iterations = iterations,
			Padding = padding
		};

		my_dungeon.Generate();

		Dungeon = my_dungeon;
	}
	
	#endregion

	#region Private Methods

	private static void ClearConsole()
	{
		var assembly = Assembly.GetAssembly(typeof(SceneView));
		var type = assembly.GetType("UnityEditor.LogEntries");
		var method = type.GetMethod("Clear");
		method.Invoke(new object(), null);
	}

	private void DebugDraw()
	{
		if (Dungeon == null)
		{
			return;
		}

		DebugDrawTree(Dungeon.DungeonTree);
	}

	private void DebugDrawTree(BinaryTree node)
	{
		while (true)
		{
			// Container
			Gizmos.color = Color.green;

			// top      
			Gizmos.DrawLine(new Vector3(node.Container.x, 0, node.Container.y), new Vector3Int(node.Container.xMax, 0, node.Container.y));

			// right
			Gizmos.DrawLine(new Vector3(node.Container.xMax, 0, node.Container.y), new Vector3Int(node.Container.xMax, 0, node.Container.yMax));

			// bottom
			Gizmos.DrawLine(new Vector3(node.Container.x, 0, node.Container.yMax), new Vector3Int(node.Container.xMax, 0, node.Container.yMax));

			// left
			Gizmos.DrawLine(new Vector3(node.Container.x, 0, node.Container.y), new Vector3Int(node.Container.x, 0, node.Container.yMax));

			// Room
			Gizmos.color = Color.red;
			// top      
			Gizmos.DrawLine(new Vector3(node.Room.x, 0, node.Room.y), new Vector3Int(node.Room.xMax, 0, node.Room.y));
			// right
			Gizmos.DrawLine(new Vector3(node.Room.xMax, 0, node.Room.y), new Vector3Int(node.Room.xMax, 0, node.Room.yMax));
			// bottom
			Gizmos.DrawLine(new Vector3(node.Room.x, 0, node.Room.yMax), new Vector3Int(node.Room.xMax, 0, node.Room.yMax));
			// left
			Gizmos.DrawLine(new Vector3(node.Room.x, 0, node.Room.y), new Vector3Int(node.Room.x, 0, node.Room.yMax));

			// Tunnel
			Gizmos.color = Color.blue;

			// top      
			Gizmos.DrawLine(new Vector3(node.Tunnel.x, 0, node.Tunnel.y), new Vector3Int(node.Tunnel.xMax, 0, node.Tunnel.y));

			// right
			Gizmos.DrawLine(new Vector3(node.Tunnel.xMax, 0, node.Tunnel.y), new Vector3Int(node.Tunnel.xMax, 0, node.Tunnel.yMax));

			// bottom
			Gizmos.DrawLine(new Vector3(node.Tunnel.x, 0, node.Tunnel.yMax), new Vector3Int(node.Tunnel.xMax, 0, node.Tunnel.yMax));

			// left
			Gizmos.DrawLine(new Vector3(node.Tunnel.x, 0, node.Tunnel.y), new Vector3Int(node.Tunnel.x, 0, node.Tunnel.yMax));

			// children
			if (node.LeftNode != null)
			{
				DebugDrawTree(node.LeftNode);
			}

			if (node.RightNode != null)
			{
				node = node.RightNode;
				continue;
			}

			break;
		}
	}

	#endregion
}