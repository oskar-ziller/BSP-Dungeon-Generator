using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;



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


public class Dungeon
{
    public BinaryTree DungeonTree;

    public float RoomMinRatio { get; set; }
    public float RoomMaxRatio { get; set; }
    public int TunnelSize { get; set; }
    public int TotalSize { get; set; }
    public int Padding { get; set; }

    public RectInt StartingRect { get; set; }
    public int Iterations { get; set; }


    public Dungeon()
    {
    }


    /// <summary>
    /// Split the DungeonTree and create rooms and tunnels
    /// </summary>
    public void Generate()
    {
        DungeonTree = BinaryTree.SplitIteratively(Iterations, StartingRect);
        CreateRooms();
        CreateTunnels();

    }

    /// Create a room in every leaf that fits its container.
    private void CreateRooms()
    {
        foreach (BinaryTree ch in DungeonTree.GetAllLeafs())
        {
            RectInt c = ch.Container;
            int randomW = (int)Random.Range(c.width * RoomMinRatio, c.width * RoomMaxRatio);
            int randomH = (int)Random.Range(c.height * RoomMinRatio, c.height * RoomMaxRatio);

            int randomX = (int)Random.Range(0, c.width - randomW);
            int randomY = (int)Random.Range(0, c.height - randomH);

            ch.Room = new RectInt(c.x + randomX, c.y + randomY, randomW, randomH);
        }
    }

    // Create tunnels by 1st connecting leafs with eachother and then
    // connecting every node with each other
    private void CreateTunnels()
    {
        // connect leafs
        foreach (BinaryTree ch in DungeonTree.GetAllLeafs())
        {
            BinaryTree parent = ch.Parent;

            if (parent.Tunnel.Equals(new RectInt()))
            {
                ConnectChildren(parent);
            }
        }

        // connect nodes
        foreach (BinaryTree ch in DungeonTree.GetAllNonLeafs())
        {
            BinaryTree parent = ch.Parent;

            if (parent.Tunnel.Equals(new RectInt()))
            {
                ConnectChildren(parent);
            }
        }
    }

    /// <summary>
    /// Connects Left and Right children of a node.
    /// </summary>
    /// <param name="main">Root tree</param>
    private void ConnectChildren(BinaryTree node)
    {
        List<RectInt> possibleTunnelStarts = new List<RectInt>();

        // If parent is split horizontal we want a vertical tunnel or vice versa
        TunnelDirection direction = node.SplitDir == SplitDirection.Vertical ? TunnelDirection.Horizontal : TunnelDirection.Vertical;


        // iterate all rooms and save every point a tunnel can fit
        // takes into account the Padding value
        foreach (RectInt room in node.LeftNode.GetAllRooms())
        {
            int roomStart = 0;
            int roomEnd = 0;

            // Set roomStart after Padding units 
            // Subtract Padding and TunnelSize from actual room end

            // If we want a Vertical tunnel, we iterate from xMin to xMax
            if (direction == TunnelDirection.Vertical)
            {
                roomStart = room.xMin + Padding;
                roomEnd = room.xMax - Padding * 2 - TunnelSize;
            }

            // If we want a Horizontal tunnel, we iterate from yMin to yMax
            if (direction == TunnelDirection.Horizontal)
            {
                roomStart = room.yMin;
                roomEnd = room.yMax - Padding * 2 - TunnelSize;
            }

            for (int pos = roomStart; pos <= roomEnd; pos++)
            {
                // For every location, create and save a tunnelStart with zero thickness
                //Debug.Log($"adding possible {direction} tunnel start at pos: {pos} room: {room}");

                if (direction == TunnelDirection.Vertical)
                {
                    possibleTunnelStarts.Add(new RectInt(pos, room.yMax, TunnelSize + Padding * 2, 0));
                }

                if (direction == TunnelDirection.Horizontal)
                {
                    possibleTunnelStarts.Add(new RectInt(room.xMax, pos, 0, TunnelSize + Padding * 2));
                }
            }
        }

        // Do the same for every tunnel
        foreach (RectInt tunnel in node.LeftNode.GetAllTunnels())
        {
            int tunnelStart = 0;
            int tunnelEnd = 0;

            if (direction == TunnelDirection.Vertical)
            {
                tunnelStart = tunnel.xMin;
                tunnelEnd = tunnel.xMax - Padding * 2 - TunnelSize;
            }

            if (direction == TunnelDirection.Horizontal)
            {
                tunnelStart = tunnel.yMin;
                tunnelEnd = tunnel.yMax - Padding * 2 - TunnelSize;
            }

            for (int pos = tunnelStart; pos <= tunnelEnd; pos++)
            {
                //Debug.Log($"adding possible {direction} tunnel start at tunnel: " + tunnel.ToString());

                if (direction == TunnelDirection.Vertical)
                {
                    possibleTunnelStarts.Add(new RectInt(pos, tunnel.yMax, TunnelSize + Padding * 2, 0));
                }

                if (direction == TunnelDirection.Horizontal)
                {
                    possibleTunnelStarts.Add(new RectInt(tunnel.xMax, pos, 0, TunnelSize + Padding * 2));
                }
            }
        }

        List<RectInt> tunnels = new List<RectInt>();

        // iterate possibleTunnelStarts and see if they also fit at RightNode tunnels or rooms

        // rooms
        foreach (RectInt st in possibleTunnelStarts)
        {
            foreach (RectInt room in node.RightNode.GetAllRooms())
            {
                Vector2Int pointToCheck = new Vector2Int();
                Vector2Int pointToCheck2 = new Vector2Int();

                // Set pointToCheck to starting X of the possible tunnel and Y of the room we are checking
                // Set pointToCheck2 to the desired width
                if (direction == TunnelDirection.Vertical)
                {
                    pointToCheck = new Vector2Int(st.x, room.yMin);
                    pointToCheck2 = new Vector2Int(st.x + st.width, room.yMin);
                }

                if (direction == TunnelDirection.Horizontal)
                {
                    pointToCheck = new Vector2Int(room.xMin, st.y);
                    pointToCheck2 = new Vector2Int(room.xMin, st.y + st.height);
                }

                // if room contains both points, possibleTunnel fits the room
                if (room.Contains(pointToCheck) && room.Contains(pointToCheck2))
                {
                    // resize the tunnel so it touches both rooms
                    st.SetMinMax(st.min, pointToCheck2);
                    tunnels.Add(st);
                }
            }
        }

        // tunnels - do the same with rooms
        foreach (RectInt st in possibleTunnelStarts)
        {
            foreach (RectInt tunnel in node.RightNode.GetAllTunnels())
            {
                Vector2Int pointToCheck = new Vector2Int();
                Vector2Int pointToCheck2 = new Vector2Int();

                if (direction == TunnelDirection.Vertical)
                {
                    pointToCheck = new Vector2Int(st.x, tunnel.yMin);
                    pointToCheck2 = new Vector2Int(st.x + st.width, tunnel.yMin);
                }

                if (direction == TunnelDirection.Horizontal)
                {
                    pointToCheck = new Vector2Int(tunnel.xMin, st.y);
                    pointToCheck2 = new Vector2Int(tunnel.xMin, st.y + st.height);
                }

                if (tunnel.Contains(pointToCheck) && tunnel.Contains(pointToCheck2))
                {
                    st.SetMinMax(st.min, pointToCheck2);
                    tunnels.Add(st);
                }
            }
        }


        List<RectInt> refinedTunnels = new List<RectInt>();

        // iterate all tunnels we found and see if they collide with other rooms or tunnels in other nodes
        foreach (RectInt t in tunnels)
        {
            bool collision = false;

            foreach (BinaryTree c in DungeonTree.GetAllChildren())
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
                refinedTunnels.Add(t);
            }
        }

        List<RectInt> final = new List<RectInt>();

        // finally, remove the Padding from the tunnels
        for (int i = 0; i < refinedTunnels.Count; i++)
        {
            RectInt r = refinedTunnels[i];

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

        node.Tunnel = final[Random.Range(0, refinedTunnels.Count - 1)];
    }

}

public class BinaryTree
{
    public BinaryTree Parent;
    public BinaryTree LeftNode;
    public BinaryTree RightNode;

    public RectInt Container;
    public RectInt Room;
    public RectInt Tunnel;

    public SplitDirection SplitDir;

    public BinaryTree(RectInt rect)
    {
        Container = rect;
    }

    internal List<RectInt> GetAllRooms()
    {
        List<RectInt> toReturn = new List<RectInt>();
        List<BinaryTree> children = BFS(this);

        foreach (BinaryTree c in children)
        {
            if (!c.Room.Equals(new RectInt()))
            {
                toReturn.Add(c.Room);
            }
        }

        return toReturn;
    }

    internal List<RectInt> GetAllTunnels()
    {
        List<RectInt> toReturn = new List<RectInt>();
        List<BinaryTree> children = BFS(this);

        foreach (BinaryTree c in children)
        {
            if (!c.Tunnel.Equals(new RectInt()))
            {
                toReturn.Add(c.Tunnel);
            }
        }

        return toReturn;
    }

    internal List<BinaryTree> GetAllChildren()
    {
        return BFS(this);
    }

    internal List<BinaryTree> GetAllNonLeafs()
    {
        List<BinaryTree> toReturn = new List<BinaryTree>();

        foreach (BinaryTree c in GetAllChildren())
        {
            if (c.LeftNode != null && c.RightNode != null && c.Parent != null)
            {
                toReturn.Add(c);
            }
        }

        return toReturn;
    }

    internal List<BinaryTree> GetAllLeafs()
    {
        List<BinaryTree> toReturn = new List<BinaryTree>();

        foreach (BinaryTree c in GetAllChildren())
        {
            if (c.LeftNode == null && c.RightNode == null)
            {
                toReturn.Add(c);
            }
        }

        return toReturn;
    }


    /// <summary>
    /// Splits a tree into two parts iteratively. Assigns LeftNode and RightNode to split containers.
    /// </summary>
    /// <param name="iterations">How many iterations to split</param>
    /// <param name="container">The container to do the split operations on</param>
    /// <param name="parent">Parent node (null for root)</param>
    /// <returns></returns>
    internal static BinaryTree SplitIteratively(int iterations, RectInt container, BinaryTree parent = null)
    {
        BinaryTree node = new BinaryTree(container);
        node.Parent = parent;

        if (iterations == 0)
        {
            return node;
        }

        node.Split();
        node.LeftNode = SplitIteratively(iterations - 1, node.LeftNode.Container, node);
        node.RightNode = SplitIteratively(iterations - 1, node.RightNode.Container, node);

        return node;
    }

    private void Split()
    {
        RectInt[] splitResult = SplitContainer();
        LeftNode = new BinaryTree(splitResult[0]);
        RightNode = new BinaryTree(splitResult[1]);
    }

    private RectInt[] SplitContainer()
    {
        RectInt container = Container;

        RectInt c1, c2;

        float roomMinRatio = 0.4f;
        float roomMaxRatio = 0.6f;


        bool vertical = Random.Range(0f, 1f) > 0.5f;
        vertical = container.width > container.height;


        if (vertical)
        {
            SplitDir = SplitDirection.Vertical;
            c1 = new RectInt(container.x, container.y, (int)(container.width * Random.Range(roomMinRatio, roomMaxRatio)), container.height);
            c2 = new RectInt(container.x + c1.width, container.y, container.width - c1.width, container.height);
        }
        else
        {
            SplitDir = SplitDirection.Horizontal;
            c1 = new RectInt(container.x, container.y, container.width, (int)(container.height * Random.Range(roomMinRatio, roomMaxRatio)));
            c2 = new RectInt(container.x, container.y + c1.height, container.width, container.height - c1.height);
        }

        return new RectInt[] { c1, c2 };
    }

    private static List<BinaryTree> BFS(BinaryTree tree)
    {
        List<BinaryTree> toReturn = new List<BinaryTree>();
        Queue<BinaryTree> queue = new Queue<BinaryTree>();

        queue.Enqueue(tree);

        while (queue.Count > 0)
        {
            BinaryTree current = queue.Dequeue();

            toReturn.Add(current);

            if (current.LeftNode != null)
            {
                queue.Enqueue(current.LeftNode);
            }
            if (current.RightNode != null)
            {
                queue.Enqueue(current.RightNode);
            }
        }

        return toReturn;
    }

}

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]

    [SerializeField]
    [Range(1, 400)]
    private int totalSize = 12;

    [SerializeField]
    [Range(1, 10)]
    private int iterations = 1;

    [SerializeField]
    [Range(1, 10)]
    private int tunnelSize = 3;

    [SerializeField]
    [Range(0.1f, 1f)]
    private float roomMinRatio = 0.5f;

    [SerializeField]
    [Range(0.1f, 1f)]
    private float roomMaxRatio = 1f;

    [SerializeField]
    [Range(0, 5)]
    private int padding = 1;

    [SerializeField]
    [Header("Other")]
    private bool drawDebugLines;

    [SerializeField]
    private bool clearConsoleOnGenerate;

    public Dungeon Dungeon;

    private static void ClearConsole()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
        System.Type type = assembly.GetType("UnityEditor.LogEntries");
        MethodInfo method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    public void Create()
    {
        RectInt startingRect = new RectInt(0, 0, totalSize, totalSize);

        Dungeon myDungeon = new Dungeon()
        {
            TunnelSize = tunnelSize,
            TotalSize = totalSize,
            RoomMinRatio = roomMinRatio,
            RoomMaxRatio = roomMaxRatio,
            StartingRect = startingRect,
            Iterations = iterations,
            Padding = padding
        };

        myDungeon.Generate();

        Dungeon = myDungeon;
    }


    public void CreateGUI()
    {
        if (clearConsoleOnGenerate)
        {
            ClearConsole();
        }

        RectInt startingRect = new RectInt(0, 0, totalSize, totalSize);

        Dungeon myDungeon = new Dungeon()
        {
            TunnelSize = tunnelSize,
            TotalSize = totalSize,
            RoomMinRatio = roomMinRatio,
            RoomMaxRatio = roomMaxRatio,
            StartingRect = startingRect,
            Iterations = iterations
        };

        myDungeon.Generate();

        Dungeon = myDungeon;
    }


    private void DebugDraw()
    {
        if (Dungeon == null) return;
        DebugDrawTree(Dungeon.DungeonTree);
    }

    private void DebugDrawTree(BinaryTree node)
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
        if (node.LeftNode != null) DebugDrawTree(node.LeftNode);
        if (node.RightNode != null) DebugDrawTree(node.RightNode);
    }

    private void OnDrawGizmos()
    {
        if (drawDebugLines)
        {
            DebugDraw();
        }
    }
}
