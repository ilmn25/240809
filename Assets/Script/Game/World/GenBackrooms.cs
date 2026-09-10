/// <summary>Temporary backrooms: the shared room-set-piece engine with a palette of
/// BackroomBlock room pieces (placeholder copies of the dungeon rooms) and no content.</summary>
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
}
