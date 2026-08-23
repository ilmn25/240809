using System.Collections.Generic;
using UnityEngine;

/// <summary>Tile kinds in the dungeon layout grid.</summary>
public enum DungeonTile { Empty, Floor, Wall }

/// <summary>Edge direction a socket opens toward.</summary>
public enum SocketDir { North, South, East, West }

/// <summary>A connection point on a room's edge. The doorway it carves is
/// 2*DoorHalf+1 tiles wide, centred on <see cref="pos"/>.</summary>
public struct Socket
{
    public Vector2Int pos;
    public SocketDir dir;
    public Socket(Vector2Int pos, SocketDir dir) { this.pos = pos; this.dir = dir; }
}

/// <summary>A room template: a 2D grid of tiles plus edge sockets.</summary>
public class PrefabRoom
{
    public readonly DungeonTile[,] tiles;
    public readonly List<Socket> sockets = new List<Socket>();
    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);
    public PrefabRoom(DungeonTile[,] tiles) { this.tiles = tiles; }
}

/// <summary>Core-Keeper-style procedural dungeon layout. Rooms are prefabs with a
/// 2D tile grid and edge sockets; a BFS expansion grows a dungeon from an anchor
/// room, connecting rooms socket-to-socket with bounding-box collision checks
/// against a world grid. Failed sockets are sealed with a wall tile.</summary>
public static class DungeonLayout
{
    public const int DoorHalf = 1;   // doorway width = 2*DoorHalf+1

    public static DungeonTile[,] Generate(int width, int depth, int maxRooms, int seed)
    {
        System.Random rng = new System.Random(seed);
        DungeonTile[,] grid = new DungeonTile[width, depth];

        Queue<Socket> open = new Queue<Socket>();
        HashSet<Vector2Int> used = new HashSet<Vector2Int>();
        int placed = 0;

        PrefabRoom anchor = MakeRoom(rng);
        Vector2Int anchorPos = new Vector2Int(width / 2 - anchor.Width / 2, depth / 2 - anchor.Height / 2);
        TryCommit(anchor, anchorPos, width, depth, grid);
        placed++;
        EnqueueSockets(open, anchor, anchorPos, used);

        int iterations = 0;
        while (open.Count > 0 && placed < maxRooms && iterations < maxRooms * 10)
        {
            iterations++;
            Socket worldSocket = open.Dequeue();
            if (used.Contains(worldSocket.pos)) continue;

            SocketDir needDir = Opposite(worldSocket.dir);

            PrefabRoom room = null;
            for (int t = 0; t < 12 && room == null; t++)
            {
                PrefabRoom candidate = MakeRoom(rng);
                if (HasSocket(candidate, needDir)) room = candidate;
            }

            used.Add(worldSocket.pos);

            if (room == null || !TryPlaceAt(room, worldSocket, needDir, width, depth, grid, out Vector2Int origin))
            {
                CloseSocket(grid, worldSocket);
                continue;
            }
            placed++;
            EnqueueSockets(open, room, origin, used);
        }

        return grid;
    }

    private static void EnqueueSockets(Queue<Socket> open, PrefabRoom room, Vector2Int origin, HashSet<Vector2Int> used)
    {
        foreach (Socket s in room.sockets)
        {
            Vector2Int worldPos = origin + s.pos;
            if (used.Contains(worldPos)) continue;   // the connecting socket is already accounted for
            open.Enqueue(new Socket(worldPos, s.dir));
        }
    }

    private static bool TryCommit(PrefabRoom room, Vector2Int origin, int width, int depth, DungeonTile[,] grid)
    {
        for (int ly = 0; ly < room.Height; ly++)
            for (int lx = 0; lx < room.Width; lx++)
            {
                DungeonTile b = room.tiles[lx, ly];
                if (b == DungeonTile.Empty) continue;
                int wx = origin.x + lx, wz = origin.y + ly;
                if (wx < 0 || wx >= width || wz < 0 || wz >= depth) return false;
                DungeonTile g = grid[wx, wz];
                if (g == DungeonTile.Floor && b == DungeonTile.Wall ||
                    g == DungeonTile.Wall && b == DungeonTile.Floor)
                    return false;
            }
        for (int ly = 0; ly < room.Height; ly++)
            for (int lx = 0; lx < room.Width; lx++)
            {
                DungeonTile b = room.tiles[lx, ly];
                if (b == DungeonTile.Empty) continue;
                grid[origin.x + lx, origin.y + ly] = b;
            }
        return true;
    }

    private static bool TryPlaceAt(PrefabRoom room, Socket open, SocketDir needDir, int width, int depth, DungeonTile[,] grid, out Vector2Int origin)
    {
        if (!TryGetSocket(room, needDir, out Socket match)) { origin = default; return false; }
        origin = open.pos - match.pos;
        return TryCommit(room, origin, width, depth, grid);
    }

    private static bool TryGetSocket(PrefabRoom room, SocketDir dir, out Socket socket)
    {
        foreach (Socket s in room.sockets)
            if (s.dir == dir) { socket = s; return true; }
        socket = default;
        return false;
    }

    private static bool HasSocket(PrefabRoom room, SocketDir dir) => TryGetSocket(room, dir, out _);

    private static void CloseSocket(DungeonTile[,] grid, Socket s)
    {
        for (int i = -DoorHalf; i <= DoorHalf; i++)
        {
            Vector2Int p = DoorTile(s, i);
            if (p.x >= 0 && p.x < grid.GetLength(0) && p.y >= 0 && p.y < grid.GetLength(1))
                grid[p.x, p.y] = DungeonTile.Wall;
        }
    }

    private static SocketDir Opposite(SocketDir d) => d switch
    {
        SocketDir.North => SocketDir.South,
        SocketDir.South => SocketDir.North,
        SocketDir.East => SocketDir.West,
        _ => SocketDir.East,
    };

    private static PrefabRoom MakeRoom(System.Random rng)
    {
        int w = rng.Next(8, 15);
        int h = rng.Next(8, 15);
        int cx = w / 2, cy = h / 2;

        DungeonTile[,] tiles = new DungeonTile[w, h];
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                tiles[x, y] = (x == 0 || x == w - 1 || y == 0 || y == h - 1) ? DungeonTile.Wall : DungeonTile.Floor;

        PrefabRoom room = new PrefabRoom(tiles);

        // Give the room 2-4 distinct sockets so placing it always leaves open
        // expansion directions beyond the one it was connected through.
        int socketCount = rng.Next(2, 5);
        int[] pool = { 0, 1, 2, 3 };
        for (int i = pool.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            int t = pool[i]; pool[i] = pool[j]; pool[j] = t;
        }
        bool[] edges = { false, false, false, false };
        for (int i = 0; i < socketCount; i++)
            edges[pool[i]] = true;

        if (edges[0]) AddSocket(room, new Socket(new Vector2Int(w - 1, cy), SocketDir.East));
        if (edges[1]) AddSocket(room, new Socket(new Vector2Int(0, cy), SocketDir.West));
        if (edges[2]) AddSocket(room, new Socket(new Vector2Int(cx, h - 1), SocketDir.North));
        if (edges[3]) AddSocket(room, new Socket(new Vector2Int(cx, 0), SocketDir.South));

        return room;
    }

    private static void AddSocket(PrefabRoom room, Socket s)
    {
        for (int i = -DoorHalf; i <= DoorHalf; i++)
        {
            Vector2Int p = DoorTile(s, i);
            room.tiles[p.x, p.y] = DungeonTile.Floor;
        }
        room.sockets.Add(s);
    }

    private static Vector2Int DoorTile(Socket s, int i)
        => (s.dir == SocketDir.East || s.dir == SocketDir.West)
            ? new Vector2Int(s.pos.x, s.pos.y + i)
            : new Vector2Int(s.pos.x + i, s.pos.y);
}
