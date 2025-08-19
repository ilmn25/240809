public class WorkBenchMachine: StructureMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        storage.AddItem(ID.Brick);
        storage.AddItem(ID.StonePickaxe);
        storage.AddItem(ID.MetalAxe);
        storage.AddItem(ID.Hammer);
        storage.AddItem(ID.Sword);
        return new ContainerInfo()
        {
            Health = 500,
            Loot = ID.Tree,
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