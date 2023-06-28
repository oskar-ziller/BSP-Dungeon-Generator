#region

using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

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
	[FormerlySerializedAs("tunnelSize")] [SerializeField] [Range(1, 10)] private int tunnelWidth = 3;
	[SerializeField] [Range(0.1f, 1f)] private float roomMinRatio = 0.5f;
	[SerializeField] [Range(0.1f, 1f)] private float roomMaxRatio = 1f;
	[SerializeField] [Range(0, 5)] private int padding = 1;

	[SerializeField] [Range(0, 1)] private float splitMinRatio = 0.4f;
	[SerializeField] [Range(0, 1)] private float splitMaxRatio = 0.6f;

	[Header("Other")]
	[SerializeField] private bool drawDebugLines;
	[SerializeField] private bool drawRooms;
	[SerializeField] private bool drawTunnels;
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
			TunnelSize = tunnelWidth,
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
		method?.Invoke(new object(), null);
	}

	private static void DrawContainer(BinaryTree node)
	{
		DrawRectangle(Color.green, node.RootNode);
	}

	private static void DrawRectangle(Color color, RectInt rect)
	{
		Gizmos.color = color;

		// top
		Gizmos.DrawLine(new Vector3(rect.x, 0, rect.y), new Vector3(rect.xMax, 0, rect.y));

		// right
		Gizmos.DrawLine(new Vector3(rect.xMax, 0, rect.y), new Vector3(rect.xMax, 0, rect.yMax));

		// bottom
		Gizmos.DrawLine(new Vector3(rect.x, 0, rect.yMax), new Vector3(rect.xMax, 0, rect.yMax));

		// left
		Gizmos.DrawLine(new Vector3(rect.x, 0, rect.y), new Vector3(rect.x, 0, rect.yMax));
	}

	private static void DrawRoom(BinaryTree node)
	{
		DrawRectangle(Color.red, node.DungeonRoom);
	}

	private static void DrawTunnel(BinaryTree node)
	{
		DrawRectangle(Color.yellow, node.Tunnel);
	}

	private void DebugDraw()
	{
		if (Dungeon == null)
		{
			return;
		}

		DrawTree(Dungeon.DungeonTree);
	}

	private void DrawTree(BinaryTree node)
	{
		while (true)
		{
			DrawContainer(node);

			if (drawRooms)
			{
				DrawRoom(node);
			}

			if (drawTunnels)
			{
				DrawTunnel(node);
			}

			// children
			if (node.LeftNode != null)
			{
				DrawTree(node.LeftNode);
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