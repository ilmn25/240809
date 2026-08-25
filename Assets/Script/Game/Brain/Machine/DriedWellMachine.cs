/// <summary>A dried-up stone well. Furniture: placed directly, can't be broken,
/// hammer picks it back up.</summary>
public class DriedWellMachine : FurnitureMachine
{
    public static Info CreateInfo()
    {
        return CreateFurnitureInfo(ID.DriedWell);
    }
}
