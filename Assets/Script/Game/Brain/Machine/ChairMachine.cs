/// <summary>A simple wooden chair. Furniture: placed directly, can't be broken,
/// hammer picks it back up.</summary>
public class ChairMachine : FurnitureMachine
{
    public static Info CreateInfo()
    {
        return CreateFurnitureInfo(ID.Chair);
    }
}
