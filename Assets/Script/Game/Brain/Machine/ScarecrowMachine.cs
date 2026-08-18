using UnityEngine;

/// <summary>A placeable scarecrow that keeps pigeons away from the surrounding
/// area (pigeons check for one before dropping their load). Furniture: placed
/// directly, can't be broken, hammer picks it back up.</summary>
public class ScarecrowMachine : FurnitureMachine
{
    public static Info CreateInfo()
    {
        return CreateFurnitureInfo(ID.Scarecrow);
    }
}
