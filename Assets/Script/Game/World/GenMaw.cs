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
                    GenBlocks.SetBlock(world, new Vector3Int(x, y, z), 0);
                for (int y = FloorY - 1; y >= FloorY - 3; y--)
                    GenBlocks.SetBlock(world, new Vector3Int(x, y, z), Stone);
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
            GenBlocks.PlaceEntity(world, new Vector3Int(x, GenBlocks.FindSurfaceY(world, x, z), z), id);
        }
    }
}
