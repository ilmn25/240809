using UnityEngine;

public abstract class ChestMachine : StructureMachine, IActionSecondaryInteract, IActionPrimaryResource
{ 
    public override void OnStart()
    { 
        AddModule(new StructureSpriteCullModule()); 
        AddModule(new SpriteOrbitModule()); 
        AddState(new InContainerState()
        {
            Storage = ((ContainerInfo)Info).Storage
        });
    }
    

    public void OnActionSecondary(Info info)
    {
        if (IsCurrentState<DefaultState>())
            SetState<InContainerState>();
        else 
            SetState<DefaultState>();
    }
}

public class LootChestMachine : ChestMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        Loot.Gettable(ID.Chest).AddToContainer(storage);
        return new ContainerInfo()
        {
            Health = 500,
            Loot = ID.Tree,
            SfxHit = "dig_stone",
            SfxDestroy = "dig_stone",
            Storage = storage
        };
    }
}
public class BasicChestMachine : ChestMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        return new ContainerInfo()
        {
            Health = 500,
            Loot = ID.Tree,
            SfxHit = "dig_stone",
            SfxDestroy = "dig_stone",
            Storage = storage
        };
    }
}