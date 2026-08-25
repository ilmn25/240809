/// <summary>A sturdy wooden bookshelf. Furniture: placed directly, can't be
/// broken, hammer picks it back up.</summary>
public class BookshelfMachine : FurnitureMachine
{
    public static Info CreateInfo()
    {
        return CreateFurnitureInfo(ID.Bookshelf);
    }
}
