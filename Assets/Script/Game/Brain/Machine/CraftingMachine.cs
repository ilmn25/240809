public abstract class CraftingMachine: StructureMachine, IActionSecondaryInteract
{ 
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
public class WorkbenchMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(9); 
        storage.CreateAndAddItem(ID.Spear);
        storage.CreateAndAddItem(ID.StonePickaxe); 
        storage.CreateAndAddItem(ID.Hammer); 
        return new ContainerInfo()
        {
            Health = 500,
            Loot = ID.Tree,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}