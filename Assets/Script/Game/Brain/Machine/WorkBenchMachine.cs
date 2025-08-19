public class WorkBenchMachine: StructureMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9);
        storage.AddItem("brick");
        storage.AddItem("axe_stone");
        storage.AddItem("axe_metal");
        storage.AddItem("hammer");
        storage.AddItem("pistol");
        storage.AddItem("spear");
        storage.AddItem("minigun");
        storage.AddItem("sword");
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