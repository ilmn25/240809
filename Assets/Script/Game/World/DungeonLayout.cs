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

/// <summary>A room template: a 2D grid of tiles plus edge sockets. When
/// <see cref="piece"/> is set the room is an authored set piece — the tiles are its
/// walkable footprint and the piece itself is pasted when the layout is rendered.</summary>
public class PrefabRoom
{
    public readonly DungeonTile[,] tiles;
    public readonly List<Socket> sockets = new List<Socket>();
    public Chunk piece;
    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);
    public PrefabRoom(DungeonTile[,] tiles) { this.tiles = tiles; }
}

/// <summary>A room the layout committed, and the world origin it sits at.</summary>
public struct RoomPlacement
{
    public PrefabRoom room;
    public Vector2Int origin;
}

/// <summary>The result of a layout run: the tile grid (occupied footprints and floors),
/// the rooms in growth order, and the doorways to carve between them.</summary>
public class DungeonPlan
{
    public DungeonTile[,] grid;
    public readonly List<RoomPlacement> rooms = new List<RoomPlacement>();
    /// <summary>Doorway anchors in world space — the socket a connection was made
    /// through, pointing from the near room toward the far one.</summary>
    public readonly List<Socket> doors = new List<Socket>();
}

/// <summary>Core-Keeper-style procedural layout built from authored set pieces: each
/// piece becomes a room template (its walkable footprint) with sockets opened on 2-4 of
/// its edges, and a BFS expansion grows the dungeon from a centred anchor room,
/// connecting rooms socket-to-socket so every one lands at an irregular offset — never
/// on a lattice. Failing sockets simply stay walled. <see cref="Render"/> then pastes
/// the pieces and carves the doorways.</summary>
public static class DungeonLayout
{
    public const int DoorHalf = 1;      // doorway width = 2*DoorHalf+1
    public const int DoorBottom = 1;    // room interiors are walkable from y=1
    public const int DoorTop = 3;

    private const int SocketTries = 12;

    /// <summary>Grows a dungeon inside a <paramref name="width"/> x
    /// <paramref name="depth"/> tile grid. <paramref name="pieces"/> are the room set
    /// pieces to fill it with; <paramref name="anchorPiece"/> (optional) is forced into
    /// the centred spawn room and pasted there, so the world centre is its centre. When
    /// no anchor piece is given a random room only reserves the footprint — callers that
    /// lay their own piece over the spawn (the dungeon's exit stairwell) want that.</summary>
    public static DungeonPlan Generate(int width, int depth, int maxRooms, int seed,
        IList<Chunk> pieces, Chunk anchorPiece = null)
    {
        System.Random rng = new System.Random(seed);
        DungeonPlan plan = new DungeonPlan { grid = new DungeonTile[width, depth] };
        if (pieces == null || pieces.Count == 0) return plan;

        Queue<Socket> open = new Queue<Socket>();
        HashSet<Vector2Int> used = new HashSet<Vector2Int>();

        // The anchor room is committed centred on the world, so the world centre is its
        // centre — that is where the player spawns.
        PrefabRoom anchor = anchorPiece != null ? FromPiece(anchorPiece, rng) : MakeRoom(pieces, rng);
        Vector2Int anchorOrigin = new Vector2Int(width / 2 - anchor.Width / 2,
                                                 depth / 2 - anchor.Height / 2);
        Commit(anchor, anchorOrigin, plan);
        // An authored anchor piece IS the spawn room — it must be pasted, not merely
        // reserved, or the room never appears (Render only pastes plan.rooms).
        if (anchorPiece != null)
            plan.rooms.Add(new RoomPlacement { room = anchor, origin = anchorOrigin });
        EnqueueSockets(open, anchor, anchorOrigin, used);

        int iterations = 0;
        while (open.Count > 0 && plan.rooms.Count < maxRooms && iterations < maxRooms * 10)
        {
            iterations++;
            Socket worldSocket = open.Dequeue();
            if (used.Contains(worldSocket.pos)) continue;

            SocketDir needDir = Opposite(worldSocket.dir);
            PrefabRoom room = null;
            for (int t = 0; t < SocketTries && room == null; t++)
            {
                PrefabRoom candidate = MakeRoom(pieces, rng);
                if (HasSocket(candidate, needDir)) room = candidate;
            }

            used.Add(worldSocket.pos);

            // No room carrying a matching socket, or nowhere to put it: the socket dies
            // and the wall stays closed.
            if (room == null || !TryPlaceAt(room, worldSocket, needDir, plan, out Vector2Int origin))
                continue;

            plan.rooms.Add(new RoomPlacement { room = room, origin = origin });
            plan.doors.Add(worldSocket);
            // The far room's matching socket is one step past the near one; mark it used
            // so the expansion can't come straight back through it.
            used.Add(worldSocket.pos + Step(worldSocket.dir));
            EnqueueSockets(open, room, origin, used);
        }
        return plan;
    }

    /// <summary>Pastes every placed room's piece, then carves the doorways between them:
    /// a slot running from inside the near room, through both rooms' edges, to inside the
    /// far one — so walled and open-edged pieces connect either way.</summary>
    public static void Render(World world, DungeonPlan plan)
    {
        foreach (RoomPlacement placement in plan.rooms)
        {
            if (placement.room.piece == null) continue;
            SetPiece.Paste(world, new Vector3Int(placement.origin.x, 0, placement.origin.y),
                           placement.room.piece);
        }

        foreach (Socket door in plan.doors)
        {
            Vector2Int outward = Step(door.dir);
            for (int step = -1; step <= 2; step++)
            {
                Vector2Int cell = door.pos + outward * step;
                for (int i = -DoorHalf; i <= DoorHalf; i++)
                {
                    Vector2Int p = DoorTile(cell, door.dir, i);
                    for (int y = DoorBottom; y <= DoorTop; y++)
                        GenBlocks.SetBlock(world, p.x, y, p.y, 0);
                }
            }
        }
    }

    /// <summary>Derives a room template from an authored set piece: the tiles are its
    /// walkable footprint (fillable air at y=1) and 2-4 of its edges open as sockets.</summary>
    public static PrefabRoom FromPiece(Chunk piece, System.Random rng)
    {
        int size = piece.size;
        DungeonTile[,] tiles = new DungeonTile[size, size];
        for (int z = 0; z < size; z++)
            for (int x = 0; x < size; x++)
                tiles[x, z] = piece[x, 1, z] == 0 ? DungeonTile.Floor : DungeonTile.Wall;

        PrefabRoom room = new PrefabRoom(tiles) { piece = piece };

        int[] pool = { 0, 1, 2, 3 };
        for (int i = pool.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        int socketCount = rng.Next(2, 5);
        for (int i = 0; i < socketCount; i++)
            room.sockets.Add(MakeSocket(room, (SocketDir)pool[i], rng));
        return room;
    }

    /// <summary>A socket centred on a lateral position of the given edge, kept clear of
    /// the room's corners so the doorway never opens into one.</summary>
    private static Socket MakeSocket(PrefabRoom room, SocketDir dir, System.Random rng)
    {
        bool vertical = dir == SocketDir.North || dir == SocketDir.South;
        int span = vertical ? room.Width : room.Height;
        int edge = vertical ? room.Height - 1 : room.Width - 1;

        int lo = 1 + DoorHalf, hi = span - 1 - DoorHalf;
        if (hi < lo) { lo = hi = span / 2; }
        int lateral = rng.Next(lo, hi + 1);

        Vector2Int pos = vertical
            ? new Vector2Int(lateral, dir == SocketDir.North ? edge : 0)
            : new Vector2Int(dir == SocketDir.East ? edge : 0, lateral);
        return new Socket(pos, dir);
    }

    private static PrefabRoom MakeRoom(IList<Chunk> pieces, System.Random rng)
        => FromPiece(pieces[rng.Next(pieces.Count)], rng);

    private static void EnqueueSockets(Queue<Socket> open, PrefabRoom room, Vector2Int origin,
        HashSet<Vector2Int> used)
    {
        foreach (Socket s in room.sockets)
        {
            Vector2Int worldPos = origin + s.pos;
            if (used.Contains(worldPos)) continue;   // the socket we were connected through
            open.Enqueue(new Socket(worldPos, s.dir));
        }
    }

    /// <summary>Writes a room's footprint into the grid, unconditionally.</summary>
    private static void Commit(PrefabRoom room, Vector2Int origin, DungeonPlan plan)
    {
        int w = plan.grid.GetLength(0), d = plan.grid.GetLength(1);
        for (int lz = 0; lz < room.Height; lz++)
            for (int lx = 0; lx < room.Width; lx++)
            {
                DungeonTile t = room.tiles[lx, lz];
                if (t == DungeonTile.Empty) continue;
                int wx = origin.x + lx, wz = origin.y + lz;
                if (wx < 0 || wx >= w || wz < 0 || wz >= d) continue;
                plan.grid[wx, wz] = t;
            }
    }

    /// <summary>Places a room ADJACENT to an open socket (never overlapping it) and
    /// commits it when nothing overlaps. Adjacency is what lets open-edged pieces such
    /// as the cave rooms sit next to walled ones.</summary>
    private static bool TryPlaceAt(PrefabRoom room, Socket open, SocketDir needDir,
        DungeonPlan plan, out Vector2Int origin)
    {
        if (!TryGetSocket(room, needDir, out Socket match)) { origin = default; return false; }
        origin = open.pos + Step(open.dir) - match.pos;
        return TryCommit(room, origin, plan);
    }

    /// <summary>Commits a room unless one of its tiles lands outside the grid or on a
    /// tile another room already occupies.</summary>
    private static bool TryCommit(PrefabRoom room, Vector2Int origin, DungeonPlan plan)
    {
        int w = plan.grid.GetLength(0), d = plan.grid.GetLength(1);
        for (int lz = 0; lz < room.Height; lz++)
            for (int lx = 0; lx < room.Width; lx++)
            {
                if (room.tiles[lx, lz] == DungeonTile.Empty) continue;
                int wx = origin.x + lx, wz = origin.y + lz;
                if (wx < 0 || wx >= w || wz < 0 || wz >= d) return false;
                if (plan.grid[wx, wz] != DungeonTile.Empty) return false;
            }
        Commit(room, origin, plan);
        return true;
    }

    private static bool TryGetSocket(PrefabRoom room, SocketDir dir, out Socket socket)
    {
        foreach (Socket s in room.sockets)
            if (s.dir == dir) { socket = s; return true; }
        socket = default;
        return false;
    }

    private static bool HasSocket(PrefabRoom room, SocketDir dir) => TryGetSocket(room, dir, out _);

    private static SocketDir Opposite(SocketDir d) => d switch
    {
        SocketDir.North => SocketDir.South,
        SocketDir.South => SocketDir.North,
        SocketDir.East => SocketDir.West,
        _ => SocketDir.East,
    };

    /// <summary>One tile step in the given direction (tile space: x is world x, y is
    /// world z).</summary>
    public static Vector2Int Step(SocketDir dir) => dir switch
    {
        SocketDir.North => new Vector2Int(0, 1),
        SocketDir.South => new Vector2Int(0, -1),
        SocketDir.East => new Vector2Int(1, 0),
        _ => new Vector2Int(-1, 0),
    };

    /// <summary>The tile <paramref name="i"/> steps sideways from a socket centre, along
    /// the edge the socket sits on.</summary>
    public static Vector2Int DoorTile(Vector2Int pos, SocketDir dir, int i)
        => (dir == SocketDir.East || dir == SocketDir.West)
            ? new Vector2Int(pos.x, pos.y + i)
            : new Vector2Int(pos.x + i, pos.y);
}
