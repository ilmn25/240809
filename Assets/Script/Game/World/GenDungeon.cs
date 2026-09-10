using System.Collections.Generic;
using UnityEngine;

/// <summary>The dungeon: a cluster of room set pieces carved out of solid stone, with
/// the exit stairwell at the spawn, a boss room at the far end, loot side rooms and
/// scattered dungeon clutter.</summary>
public class GenDungeon : GenRoomLayout
{
    private const int RoomTotal = 70;
    private const int LootRooms = 4;
    private const int LootSpread = 3;   // minimum cells between loot rooms, the spawn and the boss

    // Floor clutter; thresholds are cumulative, checked in order.
    private static readonly (float Chance, ID Id)[] Scatter =
    {
        (0.012f, ID.Rubble),
        (0.006f, ID.OilBarrel),
        (0.010f, ID.OldPot),
        (0.006f, ID.Skeleton),
    };

    private static readonly string[] Palette =
    {
        "DungeonRoom", "DungeonRoomCross", "DungeonRoomPillars",
        "DungeonRoomCave", "DungeonRoomCells",
    };

    private static readonly Chunk Exit = SetPiece.LoadSetPieceFile("DungeonExit");
    private static readonly Chunk Boss = SetPiece.LoadSetPieceFile("DungeonRoomBoss");
    private static readonly Chunk LootRoom = SetPiece.LoadSetPieceFile("DungeonRoomLoot");

    private static int _stoneId;
    private static int Stone => _stoneId == 0 ? Block.ConvertID(ID.StoneBlock) : _stoneId;

    private readonly HashSet<Vector2Int> _lootCells = new HashSet<Vector2Int>();
    private Vector2Int _bossCell;

    protected override int MatrixBlock => Stone;
    protected override string[] RoomNames => Palette;
    protected override string LayoutSalt => "DungeonLayout";
    protected override int RoomCount => RoomTotal;

    /// <summary>The spawn sits on the exit stairwell's doorway (piece-local 6,11).</summary>
    protected override Vector2Int SpawnOffset => new Vector2Int(6, 11);

    /// <summary>Reserves the exit, boss and loot cells before the rooms are placed.</summary>
    protected override void PlanRooms()
    {
        // Boss room: the cluster cell farthest from the exit.
        _bossCell = SpawnCell;
        long farthest = -1;
        foreach (Vector2Int cell in Cluster)
        {
            long distance = SqrDist(cell, SpawnCell);
            if (distance > farthest) { farthest = distance; _bossCell = cell; }
        }

        // Loot rooms: a spread of cells clearing the spawn, the boss and each other.
        _lootCells.Clear();
        foreach (Vector2Int cell in Shuffled(Cluster))
        {
            if (_lootCells.Count >= LootRooms) break;
            if (cell == SpawnCell || cell == _bossCell) continue;
            if (TooClose(cell, SpawnCell) || TooClose(cell, _bossCell)) continue;

            bool clash = false;
            foreach (Vector2Int loot in _lootCells)
                if (TooClose(cell, loot)) { clash = true; break; }
            if (!clash) _lootCells.Add(cell);
        }
    }

    protected override Chunk PickRoom(Vector2Int cell)
        => cell == SpawnCell ? Exit
         : cell == _bossCell ? Boss
         : _lootCells.Contains(cell) ? LootRoom
         : base.PickRoom(cell);

    protected override void BuildContent(World world)
    {
        foreach (Vector2Int cell in Cluster)
        {
            Chunk room = PieceAt(cell);
            if (room != null) ScatterRoom(world, cell, room);
        }
    }

    /// <summary>Drops the floor clutter on the room's walkable tiles.</summary>
    private void ScatterRoom(World world, Vector2Int cell, Chunk room)
    {
        for (int x = 1; x < room.size - 1; x++)
            for (int z = 1; z < room.size - 1; z++)
            {
                if (!IsAir(room, x, 1, z) || IsAir(room, x, 0, z)) continue;   // air above a solid floor

                float roll = (float)Rng.NextDouble();
                float acc = 0f;
                foreach ((float chance, ID id) in Scatter)
                {
                    acc += chance;
                    if (roll < acc)
                    {
                        GenBlocks.PlaceEntity(world, new Vector3Int(cell.x * Cell + x, 1, cell.y * Cell + z), id);
                        break;
                    }
                }
            }
    }

    private static long SqrDist(Vector2Int a, Vector2Int b)
    {
        long dx = a.x - b.x, dz = a.y - b.y;
        return dx * dx + dz * dz;
    }

    private static bool TooClose(Vector2Int a, Vector2Int b)
        => SqrDist(a, b) < (long)LootSpread * LootSpread;
}
