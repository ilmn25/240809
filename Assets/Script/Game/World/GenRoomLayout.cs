using System.Collections.Generic;
using UnityEngine;

/// <summary>Shared generator for worlds built from a grid of room set pieces joined by
/// carved doorways (the dungeon, the backrooms). The world starts as one solid
/// <see cref="MatrixBlock"/> matrix; a connected cluster of 15x15 cells is filled with
/// room pieces and doorways are punched through the shared walls. Subclasses supply the
/// palette and any extra content (exit, boss, loot...).</summary>
public abstract class GenRoomLayout : Gen
{
    /// <summary>Room footprint and cell pitch (one chunk).</summary>
    protected const int Cell = World.ChunkSize;

    private const int DoorBottom = 1;       // room pieces are walkable from y=1
    private const int DoorTop = 3;
    private const double LoopChance = 0.25; // extra doors beyond the spanning tree

    private static readonly Vector2Int[] Dirs =
    {
        Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down,
    };

    /// <summary>Solid block the whole matrix is filled with; cells without a room stay this.</summary>
    protected abstract int MatrixBlock { get; }

    /// <summary>Names of the room pieces scattered through the cluster.</summary>
    protected abstract string[] RoomNames { get; }

    /// <summary>Salt for the deterministic layout seed.</summary>
    protected virtual string LayoutSalt => "RoomLayout";

    /// <summary>How many cells to fill with rooms, grown as one connected cluster.</summary>
    protected virtual int RoomCount => CellsX * CellsZ;

    /// <summary>Blocks to inset the spawn from the spawn cell's corner.</summary>
    protected virtual Vector2Int SpawnOffset => new Vector2Int(2, 2);

    /// <summary>The loaded room pieces (<see cref="RoomNames"/> entries that failed to load are dropped).</summary>
    protected Chunk[] Rooms { get; private set; }

    /// <summary>How many cells the world is divided into.</summary>
    protected int CellsX { get; private set; }
    protected int CellsZ { get; private set; }

    /// <summary>The cells filled with rooms, in growth order.</summary>
    protected List<Vector2Int> Cluster { get; private set; }

    protected System.Random Rng { get; private set; }

    /// <summary>The cell the player spawns in (the world centre).</summary>
    protected Vector2Int SpawnCell => new Vector2Int(GetSize().x / 2, GetSize().z / 2);

    /// <summary>The plain room — the first entry in <see cref="RoomNames"/>.</summary>
    protected Chunk PlainRoom => Rooms.Length > 0 ? Rooms[0] : null;

    private bool[,] _filled;
    private Chunk[,] _pieces;
    private readonly List<(Vector2Int A, Vector2Int B)> _treeEdges = new List<(Vector2Int, Vector2Int)>();

    private static int _overlayId;

    public override Vector3Int GetSize() => new Vector3Int(20, 1, 20);

    public override Vector3Int GetSpawnPoint()
    {
        Vector2Int spawn = SpawnCell * Cell + SpawnOffset;
        return new Vector3Int(spawn.x, 2, spawn.y);
    }

    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        if (currentCoordinate.y != 0) return;

        // Solid matrix that the rooms are carved out of.
        int cs = World.ChunkSize;
        for (int y = 0; y < cs; y++)
            for (int x = 0; x < cs; x++)
                for (int z = 0; z < cs; z++)
                    currentChunk[x, y, z] = MatrixBlock;
    }

    protected override void GenPostWorld(World world)
    {
        CellsX = world.Size.x * World.ChunkSize / Cell;
        CellsZ = world.Size.z * World.ChunkSize / Cell;
        Rng = new System.Random((int)GetDeterministicOffset(LayoutSalt));
        Rooms = LoadRooms(RoomNames);

        GrowCluster();
        PlanRooms();
        PlaceRooms(world);
        CarveDoors(world);
        BuildContent(world);
    }

    /// <summary>Called once the cluster is grown, before pieces are placed — lets
    /// subclasses reserve cells (boss, loot...) based on <see cref="Cluster"/>.</summary>
    protected virtual void PlanRooms() { }

    /// <summary>The piece for a cluster cell. Default: a random <see cref="Rooms"/>
    /// entry, with the spawn cell forced to the plain first room.</summary>
    protected virtual Chunk PickRoom(Vector2Int cell)
        => Rooms.Length == 0 ? null
         : cell == SpawnCell ? PlainRoom
         : Rooms[Rng.Next(Rooms.Length)];

    /// <summary>Pastes extra set pieces / spawns entities after the rooms and doors.</summary>
    protected virtual void BuildContent(World world) { }

    /// <summary>The piece placed in a cell, or null when the cell is solid matrix.</summary>
    protected Chunk PieceAt(Vector2Int cell) => _pieces[cell.x, cell.y];

    /// <summary>Whether a piece cell is air: fillable (0) or the non-fillable OverlayBlock.</summary>
    protected static bool IsAir(Chunk piece, int x, int y, int z)
    {
        if (_overlayId == 0) _overlayId = Block.ConvertID(ID.OverlayBlock);
        int block = piece[x, y, z];
        return block == 0 || block == _overlayId;
    }

    /// <summary>A shuffled copy of <paramref name="source"/>, using <see cref="Rng"/>.</summary>
    protected List<Vector2Int> Shuffled(List<Vector2Int> source)
    {
        List<Vector2Int> copy = new List<Vector2Int>(source);
        for (int i = copy.Count - 1; i > 0; i--)
        {
            int j = Rng.Next(i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }
        return copy;
    }

    /// <summary>Grows one connected cluster of filled cells out of the spawn cell.</summary>
    private void GrowCluster()
    {
        int target = Mathf.Clamp(RoomCount, 1, CellsX * CellsZ);
        _filled = new bool[CellsX, CellsZ];
        _pieces = new Chunk[CellsX, CellsZ];
        _treeEdges.Clear();
        Cluster = new List<Vector2Int>(target);
        List<Vector2Int> frontier = new List<Vector2Int>();

        Vector2Int start = SpawnCell;
        _filled[start.x, start.y] = true;
        Cluster.Add(start);
        frontier.Add(start);

        while (Cluster.Count < target && frontier.Count > 0)
        {
            int index = Rng.Next(frontier.Count);
            Vector2Int cell = frontier[index];
            Vector2Int next = RandomUnfilledNeighbour(cell);
            if (next.x < 0) { frontier.RemoveAt(index); continue; }

            _filled[next.x, next.y] = true;
            Cluster.Add(next);
            _treeEdges.Add((cell, next));
            frontier.Add(next);
        }
    }

    private Vector2Int RandomUnfilledNeighbour(Vector2Int cell)
    {
        int offset = Rng.Next(Dirs.Length);
        for (int i = 0; i < Dirs.Length; i++)
        {
            Vector2Int candidate = cell + Dirs[(i + offset) % Dirs.Length];
            if (InGrid(candidate) && !_filled[candidate.x, candidate.y]) return candidate;
        }
        return new Vector2Int(-1, -1);
    }

    private void PlaceRooms(World world)
    {
        foreach (Vector2Int cell in Cluster)
            Paste(world, _pieces[cell.x, cell.y] = PickRoom(cell), cell * Cell);
    }

    /// <summary>Carves the tree doorways (which keep every room reachable) plus extra loop doors.</summary>
    private void CarveDoors(World world)
    {
        HashSet<(Vector2Int, Vector2Int)> carved = new HashSet<(Vector2Int, Vector2Int)>();
        foreach ((Vector2Int a, Vector2Int b) in _treeEdges)
        {
            CarveDoor(world, a, b);
            carved.Add(EdgeKey(a, b));
        }

        foreach (Vector2Int cell in Cluster)
            foreach (Vector2Int dir in Dirs)
            {
                Vector2Int next = cell + dir;
                if (!InGrid(next) || !_filled[next.x, next.y]) continue;

                (Vector2Int, Vector2Int) key = EdgeKey(cell, next);
                if (carved.Contains(key) || Rng.NextDouble() >= LoopChance) continue;
                CarveDoor(world, cell, next);
                carved.Add(key);
            }
    }

    /// <summary>Punches a doorway between two adjacent filled cells: a 1-wide slot
    /// running from inside the near piece, through both walls (and any matrix gap), to
    /// inside the far piece, so irregular pieces are always connected.</summary>
    private void CarveDoor(World world, Vector2Int a, Vector2Int b)
    {
        bool alongX = a.x != b.x;
        Vector2Int lo = (alongX ? a.x : a.y) < (alongX ? b.x : b.y) ? a : b;
        Vector2Int hi = lo == a ? b : a;

        int sizeLo = PieceAt(lo)?.size ?? 0;
        int sizeHi = PieceAt(hi)?.size ?? 0;
        if (sizeLo <= 0 || sizeHi <= 0) return;

        int cellLo = alongX ? lo.x : lo.y;
        int cellHi = alongX ? hi.x : hi.y;
        int lateralLo = Mathf.Max(alongX ? lo.y : lo.x, alongX ? hi.y : hi.x) * Cell + 1;
        int lateralHi = Mathf.Min((alongX ? lo.y : lo.x) * Cell + sizeLo - 2,
                                  (alongX ? hi.y : hi.x) * Cell + sizeHi - 2);
        if (lateralHi < lateralLo) return;
        int lateral = Rng.Next(lateralLo, lateralHi + 1);

        int from = cellLo * Cell + 1;
        int to = cellHi * Cell + sizeHi - 2;
        for (int across = from; across <= to; across++)
            for (int y = DoorBottom; y <= DoorTop; y++)
                GenBlocks.SetBlock(world, alongX ? new Vector3Int(across, y, lateral)
                                                 : new Vector3Int(lateral, y, across), 0);
    }

    /// <summary>An order-independent key for the edge between two adjacent cells.</summary>
    private static (Vector2Int, Vector2Int) EdgeKey(Vector2Int a, Vector2Int b)
    {
        bool aFirst = a.x < b.x || (a.x == b.x && a.y < b.y);
        return aFirst ? (a, b) : (b, a);
    }

    private static void Paste(World world, Chunk setPiece, Vector2Int originXZ)
    {
        if (setPiece == null) return;
        SetPiece.Paste(world, new Vector3Int(originXZ.x, 0, originXZ.y), setPiece);
    }

    private static Chunk[] LoadRooms(string[] names)
    {
        List<Chunk> rooms = new List<Chunk>(names.Length);
        foreach (string name in names)
        {
            Chunk room = SetPiece.LoadSetPieceFile(name);
            if (room != null) rooms.Add(room);
        }
        return rooms.ToArray();
    }

    private bool InGrid(Vector2Int cell)
        => cell.x >= 0 && cell.x < CellsX && cell.y >= 0 && cell.y < CellsZ;
}
