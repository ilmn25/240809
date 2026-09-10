using UnityEngine;

/// <summary>Temporary backrooms: the shared room-set-piece engine with a palette of
/// BackroomBlock room pieces (placeholder copies of the dungeon rooms). The only
/// content is the threshold portal home, standing in the spawn room.</summary>
public class GenBackrooms : GenRoomLayout
{
    private static readonly string[] Palette =
    {
        "BackroomRoom", "BackroomRoomCross", "BackroomRoomPillars",
        "BackroomRoomCave", "BackroomRoomCells",
    };

    private static int _id;
    private static int Backroom => _id == 0 ? Block.ConvertID(ID.BackroomBlock) : _id;

    protected override int MatrixBlock => Backroom;
    protected override string[] RoomNames => Palette;
    protected override string LayoutSalt => "BackroomsLayout";

    /// <summary>Stands the return portal in the middle of the world (the spawn point).
    /// Its twin waits in the middle of the Abyss, so right-clicking either one crosses
    /// the threshold.</summary>
    protected override void BuildContent(World world)
    {
        Vector3Int spawn = GetSpawnPoint();
        // The spawn point is the middle of the spawn room, whose floor is the piece's y0.
        GenBlocks.PlaceEntity(world, new Vector3Int(spawn.x, 1, spawn.z), ID.Threshold);
    }
}
