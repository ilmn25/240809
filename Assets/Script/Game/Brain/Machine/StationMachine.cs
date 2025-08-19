public class StationMachine: StructureMachine, IActionSecondaryInteract
{    public static Info CreateInfo()
    {
        Storage storage = new Storage(27);
        storage.AddItem("blueprint");   
        storage.AddItem("workbench");
        storage.AddItem("stonecutter");
        storage.AddItem("furnace");
        storage.AddItem("chest");   
        return new ContainerInfo()
        {
            Health = 500,
            Loot = "tree",
            SfxHit = "dig_stone",
            SfxDestroy = "dig_stone",
            Storage = storage
        };
    }
    public override void OnStart()
    {
        AddModule(new StructureSpriteCullModule()); 
        AddModule(new SpriteOrbitModule()); 
        AddState(new InCraftingState());
    }
    

    public void OnActionSecondary(Info info)
    {
        if (IsCurrentState<DefaultState>())
            SetState<InCraftingState>();
        else 
            SetState<DefaultState>();
    }
} 