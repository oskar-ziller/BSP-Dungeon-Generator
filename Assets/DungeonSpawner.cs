using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



internal class TransformObj
{
    public Transform obj;
    public int x, y;

    public TransformObj(Transform o, int xpos, int ypos)
    {
        obj = o;
        x = xpos;
        y = ypos;
    }
}


public class DungeonSpawner : MonoBehaviour
{
    const string holderName = "Dungeon";
    const int cellSize = 10;

    [Range(0.1f, 5f)]
    public float FloorScale = 1;

    public Transform tilePrefab;

    List<TransformObj> floors = new List<TransformObj>();
    List<TransformObj> walls = new List<TransformObj>();

    DungeonGenerator dungeonGenerator;

    private void Awake()
    {
    }


    public void Spawn()
    {
        if (dungeonGenerator == null)
        {
            dungeonGenerator = GetComponent<DungeonGenerator>();
        }

        floors.Clear();
        walls.Clear();
        RemoveExisting();

        dungeonGenerator.Create();
        CreateFloors();
    }


    private void RemoveExisting()
    {
        if (GameObject.Find(holderName))
        {
            //Destroy(GameObject.Find(holderName).gameObject);
            DestroyImmediate(GameObject.Find(holderName).gameObject);
        }
    }

    private void CreateFloors()
    {
        Transform dungeonHolder = new GameObject(holderName).transform;
        dungeonHolder.parent = transform;

        List<RectInt> rooms = dungeonGenerator.Dungeon.DungeonTree.GetAllRooms();

        int roomCount = 0;

        foreach (RectInt room in rooms)
        {
            Transform roomHolder = new GameObject("Room " + roomCount).transform;
            roomHolder.parent = dungeonHolder;
            roomCount++;

            for (int x = room.x; x < room.xMax; x++)
            {
                for (int y = room.y; y < room.yMax; y++)
                {
                    SpawnFloorTileAt(x, y, roomHolder);
                }
            }
        }




        List<RectInt> tunnels = dungeonGenerator.Dungeon.DungeonTree.GetAllTunnels();

        int tunnelCount = 0;

        foreach (RectInt tunnel in tunnels)
        {
            Transform tunnelHolder = new GameObject("Tunnel " + tunnelCount).transform;
            tunnelHolder.parent = dungeonHolder;
            tunnelCount++;

            for (int x = tunnel.x; x < tunnel.xMax; x++)
            {
                for (int y = tunnel.y; y < tunnel.yMax; y++)
                {
                    SpawnFloorTileAt(x, y, tunnelHolder);
                }
            }
        }

    }

    private void SpawnFloorTileAt(int x, int y, Transform parent)
    {
        Transform newTile = Instantiate(tilePrefab, GetWorldPosition(x, y), Quaternion.identity);
        newTile.parent = parent;

        newTile.localScale = new Vector3(FloorScale, FloorScale, FloorScale);
        //newTile.GetChild(0).gameObject.SetActive(true); // enable floor prefab

        floors.Add(new TransformObj(newTile, x, y));
    }


    Vector3 GetWorldPosition(int x, int y)
    {
        return cellSize * FloorScale * new Vector3(x, 0, y);
    }

    Vector3 GetScreenPosition(Vector3 v)
    {
        var newVec = v / cellSize / FloorScale;
        return new Vector3(newVec.x, newVec.z, 0);
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
