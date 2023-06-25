#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

public class DungeonSpawner : MonoBehaviour
{
	#region Serialized Fields

	[Range(0.1f, 5f)]
	public float FloorScale = 1;

	[FormerlySerializedAs("tilePrefab")] public GameObject TilePrefab;

	#endregion

	#region Constants and Fields

	private const string HOLDER_NAME = "Dungeon";

	private DungeonGenerator dungeonGenerator;

	private readonly List<TransformObj> floors = new();
	private readonly List<TransformObj> walls = new();

	#endregion

	#region Unity Methods

	private void Awake()
	{
	}

	// Start is called before the first frame update
	private void Start()
	{
	}

	// Update is called once per frame
	private void Update()
	{
	}

	#endregion

	#region Public Methods

	public void Spawn()
	{
		if (dungeonGenerator == null)
		{
			dungeonGenerator = GetComponent<DungeonGenerator>();
		}

		floors.Clear();
		walls.Clear();
		RemoveExisting();

		//dungeonGenerator.Create();
		CreateFloors();
	}

	#endregion

	#region Private Methods

	private void CreateFloors()
	{
		var dungeon_holder = new GameObject(HOLDER_NAME).transform;
		dungeon_holder.parent = transform;

		var rooms = dungeonGenerator.Dungeon.DungeonTree.GetAllRooms();

		var room_count = 0;

		foreach (var room in rooms)
		{
			var room_holder = new GameObject("Room " + room_count).transform;
			room_holder.parent = dungeon_holder;
			room_count++;

			for (var x = room.x; x < room.xMax; x++)
			{
				for (var y = room.y; y < room.yMax; y++)
				{
					SpawnFloorTileAt(x, y, room_holder);
				}
			}
		}

		var tunnels = dungeonGenerator.Dungeon.DungeonTree.GetAllTunnels();

		var tunnel_count = 0;

		foreach (var tunnel in tunnels)
		{
			var tunnel_holder = new GameObject("Tunnel " + tunnel_count).transform;
			tunnel_holder.parent = dungeon_holder;
			tunnel_count++;

			for (var x = tunnel.x; x < tunnel.xMax; x++)
			{
				for (var y = tunnel.y; y < tunnel.yMax; y++)
				{
					SpawnFloorTileAt(x, y, tunnel_holder);
				}
			}
		}
	}

	private Vector3 GetScreenPosition(Vector3 v)
	{
		var new_vec = v / FloorScale;
		return new Vector3(new_vec.x, new_vec.z, 0);
	}

	private Vector3 GetWorldPosition(int x, int y)
	{
		return FloorScale * new Vector3(x + FloorScale * .5f, 0, y + FloorScale * .5f);
	}

	private void RemoveExisting()
	{
		if (GameObject.Find(HOLDER_NAME))
		{
			//Destroy(GameObject.Find(holderName).gameObject);
			DestroyImmediate(GameObject.Find(HOLDER_NAME).gameObject);
		}
	}

	private void SpawnFloorTileAt(int x, int y, Transform parent)
	{
		var new_tile = Instantiate(TilePrefab);
		new_tile.transform.position = GetWorldPosition(x, y);
		new_tile.transform.parent = parent;

		new_tile.transform.localScale = FloorScale * Vector3.one;

		floors.Add(new TransformObj(new_tile.transform, x, y));
	}

	#endregion
}