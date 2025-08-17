public class WorkBenchMachine: StructureMachine, IActionSecondary
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(27);
        storage.AddItem("brick");
        storage.AddItem("axe");
        storage.AddItem("hammer");
        storage.AddItem("pistol");
        storage.AddItem("spear");
        storage.AddItem("minigun");
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

    public void OnActionSecondary(EntityMachine entityMachine)
    {
        if (IsCurrentState<DefaultState>())
            SetState<InCraftingState>();
        else 
            SetState<DefaultState>();
    }
}

public class InCraftingState : State
{
    public override void OnEnterState()
    {
        Audio.PlaySFX("text", 0.5f);
        GUIMain.Crafting.Storage = ((ContainerInfo)Info).Storage;
        GUIMain.RefreshStorage(); 
        GUIMain.Show(true);
        GUIMain.Crafting.Show(true, !GUIMain.Showing);
    }

    public override void OnUpdateState()
    {
        if (!GUIMain.Showing || Helper.SquaredDistance(Game.Player.transform.position, Machine.transform.position) > 36) { //walk away from npc
            Machine.SetState<DefaultState>();
        }
    }

    public override void OnExitState()
    {
        GUIMain.Crafting.Show(false);
    }
}