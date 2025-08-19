public class FurnaceMachine: StructureMachine, IActionSecondaryInteract
{ 
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        storage.AddItem(ID.Charcoal);
        storage.AddItem(ID.Steel); 
        return new ContainerInfo()
        {
            Health = 500,
            Loot = ID.Slab,
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