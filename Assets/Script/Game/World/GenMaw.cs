using UnityEngine;

/// <summary>A small, desolate rocky world — the Maw's extraction site. A single
/// large brick facility (Set/MawFacility.json) sits at the centre where the
/// company's extractors turn relics into gold. Relics, geodes and ore litter the
/// rock so there's always something to extract for the daily quota.</summary>
public class GenMaw : Gen
{
    private const int ChunksXZ = 12;
    private const int ChunksY = 3;
    private const int FloorY = 16;          // flat central ground level
    private const float IslandRadius = 78f; // how far the rock extends before void
    private const float FlatRadius = 40f;   // central area kept flat for the facility pad
    private const float HillScale = 0.045f;

    private static readonly Chunk Facility = SetPiece.LoadSetPieceFile("MawFacility");

    // Lazy: GetDeterministicOffset reads Save.Inst.seed, but Save.Inst isn't set
    // yet when this type's static ctor runs (GenMaw is constructed inside
    // Gen.Dictionary while a Save is being created). Defer the call until chunk
    // generation, when Save.Inst exists.
    private static float _hillOffset = float.NaN;
    private static float HillOffset => float.IsNaN(_hillOffset)
        ? _hillOffset = Gen.GetDeterministicOffset("MawHills")
        : _hillOffset;

    private static int _stoneId, _graniteId;
    private static int Stone => _stoneId == 0 ? Block.ConvertID(ID.StoneBlock) : _stoneId;
    private static int Granite => _graniteId == 0 ? Block.ConvertID(ID.GraniteBlock) : _graniteId;

    private static int CenterX => ChunksXZ / 2 * World.ChunkSize;
    private static int CenterZ => ChunksXZ / 2 * World.ChunkSize;

    public override Vector3Int GetSize() => new Vector3Int(ChunksXZ, ChunksY, ChunksXZ);

    public override Vector3Int GetSpawnPoint()
    {
        // High above the facility doorway; the player drops onto the pad in front.
        return new Vector3Int(CenterX, 40, CenterZ - 15);
    }

    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        float hillOffset = HillOffset; // resolve once per chunk
        for (int x = 0; x < World.ChunkSize; x++)
        {
            float wx = currentCoordinate.x + x;
            for (int z = 0; z < World.ChunkSize; z++)
            {
                float wz = currentCoordinate.z + z;
                float dx = wx - CenterX;
                float dz = wz - CenterZ;
                float d = Mathf.Sqrt(dx * dx + dz * dz);
                if (d > IslandRadius) continue; // empty void beyond the rock

                // Hills rise away from the centre; the pad stays flat.
                float hill = Mathf.PerlinNoise(wx * HillScale + hillOffset, wz * HillScale + hillOffset) * 9f;
                hill *= Mathf.Clamp01(d / FlatRadius);
                int surface = FloorY + Mathf.FloorToInt(hill);

                for (int y = 0; y < World.ChunkSize; y++)
                {
                    int wy = currentCoordinate.y + y;
                    if (wy < 0 || wy > surface) continue;
                    currentChunk[x, y, z] = wy >= surface - 2 ? Granite : Stone;
                }
            }
        }
    }

    protected override void GenPostWorld(World world)
    {
        if (Facility == null) return;

        // Flatten a stone pad for the facility (level with the flat centre).
        const int pad = 12;
        for (int x = CenterX - pad; x <= CenterX + pad; x++)
            for (int z = CenterZ - pad; z <= CenterZ + pad; z++)
            {
                for (int y = FloorY; y <= FloorY + 2; y++)
                    SetBlock(world, new Vector3Int(x, y, z), 0);
                for (int y = FloorY - 1; y >= FloorY - 3; y--)
                    SetBlock(world, new Vector3Int(x, y, z), Stone);
            }

        // The facility is a 14-cube set piece — anchor its floor to the pad.
        SetPiece.Paste(world, new Vector3Int(CenterX - Facility.size / 2, FloorY, CenterZ - Facility.size / 2), Facility);

        ScatterFodder(world);
    }

    /// <summary>Places a guaranteed handful of relics, a geode and a boulder on the
    /// pad right in front of the facility, so extraction fodder is visible the
    /// moment the player arrives (the quota needs something to feed the refinery).</summary>
    private static void ScatterFodder(World world)
    {
        (int dx, int dz, ID id)[] batch =
        {
            (-3, 0, ID.PetrifiedDelver),
            (0, 0, ID.PetrifiedDelver),
            (3, 0, ID.Geode),
            (-1, 3, ID.StarCompass),
            (2, 3, ID.ThousandMenWedge),
            (0, 6, ID.StoneBoulder),
        };
        foreach ((int dx, int dz, ID id) in batch)
        {
            // Just south of the facility, on the flat centre pad where the player spawns.
            int x = CenterX + dx;
            int z = CenterZ - 15 + dz;
            PlaceScatter(world, new Vector3Int(x, FindSurface(world, x, z), z), id);
        }
    }

    /// <summary>Adds a scatter entity to its chunk — structures to StaticEntity
    /// (persist + show on the map), loose items to DynamicEntity (spawn as items).</summary>
    private static void PlaceScatter(World world, Vector3Int cell, ID id)
    {
        if (cell.y < 0) return; // no surface found
        Info info = Entity.CreateInfo(id, cell);
        if (info == null) return;
        Chunk chunk = world[World.GetChunkCoordinate(cell)];
        if (chunk == null || chunk == Chunk.Zero) return;

        if (Entity.Dictionary.ContainsKey(id))
            chunk.StaticEntity.Add(info);
        else
            chunk.DynamicEntity.Add(info);
    }

    /// <summary>First air block directly above a solid block, or -1.</summary>
    private static int FindSurface(World world, int x, int z)
    {
        for (int y = world.Bounds.y - 1; y >= 1; y--)
        {
            Vector3Int block = new Vector3Int(x, y, z);
            Vector3Int chunkCoord = World.GetChunkCoordinate(block);
            Chunk chunk = world[chunkCoord];
            if (chunk == null || chunk == Chunk.Zero) continue;

            int localX = block.x - chunkCoord.x;
            int localY = block.y - chunkCoord.y;
            int localZ = block.z - chunkCoord.z;
            if (localY == 0) continue;

            if (chunk[localX, localY, localZ] == 0 && chunk[localX, localY - 1, localZ] != 0)
                return y;
        }
        return -1;
    }

    private static void SetBlock(World world, Vector3Int worldPos, int blockID)
    {
        if (worldPos.x < 0 || worldPos.x >= world.Bounds.x ||
            worldPos.y < 0 || worldPos.y >= world.Bounds.y ||
            worldPos.z < 0 || worldPos.z >= world.Bounds.z) return;
        Chunk chunk = world[worldPos];
        if (chunk == null || chunk == Chunk.Zero) return;
        chunk[World.GetBlockCoordinate(worldPos)] = blockID;
    }
}
