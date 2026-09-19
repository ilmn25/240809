using UnityEngine;

/// <summary>The backrooms: the same piece-backed socket layout as the dungeon (see
/// <see cref="DungeonLayout"/>), filled with the Backroom* room pieces and carved out of
/// solid BackroomBlock. The spawn room is <c>BackroomThreshold</c> (the piece carrying
/// the Computer), and the return portal always stands on its centre.</summary>
public class GenBackrooms : GenSocketRooms
{
    // The room pieces the layout fills the backrooms with.
    private static readonly Chunk[] Pieces =
    {
        SetPiece.LoadSetPieceFile("Backroom"),
        SetPiece.LoadSetPieceFile("BackroomCross"),
        SetPiece.LoadSetPieceFile("BackroomPillars"),
        SetPiece.LoadSetPieceFile("BackroomCave"),
        SetPiece.LoadSetPieceFile("BackroomCells"),
        SetPiece.LoadSetPieceFile("BackroomBoss"),
        SetPiece.LoadSetPieceFile("BackroomLoot"),
        SetPiece.LoadSetPieceFile("BackroomTreasure"),
    };

    /// <summary>The spawn room — the authored piece that carries the Computer.</summary>
    private static readonly Chunk ThresholdRoom = SetPiece.LoadSetPieceFile("BackroomThreshold");

    private static int _id;
    private static int Backroom => _id == 0 ? Block.ConvertID(ID.BackroomBlock) : _id;

    protected override int MatrixBlock => Backroom;
    protected override string LayoutSalt => "BackroomsLayout";
    protected override Chunk[] RoomPieces => Pieces;
    protected override Chunk AnchorPiece => ThresholdRoom;

    protected override void GenPostWorld(World world)
    {
        BuildLayout(world);

        // The return portal always stands on the spawn point — the centre of the anchor
        // room, which DungeonLayout places centred on the world. Its twin waits on the
        // Abyss spawn point.
        Vector3Int spawn = GetSpawnPoint();
        GenBlocks.PlaceEntity(world, new Vector3Int(spawn.x, 1, spawn.z), ID.Threshold);
    }
}
