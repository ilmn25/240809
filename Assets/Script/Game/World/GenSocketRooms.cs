using UnityEngine;

/// <summary>Shared scaffolding for the socket-room worlds — the dungeon and the backrooms.
/// The world starts as one solid <see cref="MatrixBlock"/> matrix and is filled with
/// authored set pieces laid out by <see cref="DungeonLayout"/>. There is no carving pass:
/// the pasted pieces supply the floor, ceiling and walls. A subclass supplies its palette
/// and then adds its own content in <see cref="Gen.GenPostWorld"/>.</summary>
public abstract class GenSocketRooms : Gen
{
    /// <summary>Solid block the whole world starts as; the rooms are carved out of it.</summary>
    protected abstract int MatrixBlock { get; }

    /// <summary>The authored room pieces the layout fills the world with.</summary>
    protected abstract Chunk[] RoomPieces { get; }

    /// <summary>Piece forced into the centred spawn room, or null to pick one at random.</summary>
    protected virtual Chunk AnchorPiece => null;

    /// <summary>How many rooms to grow the layout to.</summary>
    protected virtual int MaxRooms => 70;

    /// <summary>Salt for the deterministic layout seed.</summary>
    protected abstract string LayoutSalt { get; }

    /// <summary>The rendered layout of the current world; valid from <see cref="BuildLayout"/> on.</summary>
    protected DungeonPlan Plan { get; private set; }

    protected int LayoutWidth { get; private set; }
    protected int LayoutDepth { get; private set; }
    protected int LayoutSeed { get; private set; }

    public override Vector3Int GetSize() => new Vector3Int(20, 1, 20);

    /// <summary>The world centre — which is the centre of the anchor room the layout
    /// commits there, so it is where the player spawns.</summary>
    public override Vector3Int GetSpawnPoint()
    {
        int c = GetSize().x / 2 * World.ChunkSize;
        return new Vector3Int(c, 2, c);
    }

    protected override void GenChunk(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        if (currentCoordinate.y != 0) return;

        // Solid matrix that the rooms are carved into.
        int cs = World.ChunkSize;
        int block = MatrixBlock;
        for (int y = 0; y < cs; y++)
            for (int x = 0; x < cs; x++)
                for (int z = 0; z < cs; z++)
                    currentChunk[x, y, z] = block;
    }

    /// <summary>Grows the layout and renders it — pastes every room's piece and carves
    /// the doorways between them.</summary>
    protected void BuildLayout(World world)
    {
        LayoutWidth = world.Size.x * World.ChunkSize;
        LayoutDepth = world.Size.z * World.ChunkSize;
        LayoutSeed = (int)GetDeterministicOffset(LayoutSalt);
        Plan = DungeonLayout.Generate(LayoutWidth, LayoutDepth, MaxRooms, LayoutSeed,
                                      RoomPieces, AnchorPiece);
        DungeonLayout.Render(world, Plan);
    }
}
