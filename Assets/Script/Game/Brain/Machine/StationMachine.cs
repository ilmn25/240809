public class StationMachine: StructureMachine, IActionSecondary
{
    public override void OnStart()
    {
        Storage storage = new Storage(27);
        storage.AddItem("chest"); 
        storage.AddItem("workbench"); 
        AddModule(new ContainerInfo()
        {
            Health = 500,
            Loot = "tree",
            SfxHit = "dig_stone",
            SfxDestroy = "dig_stone",
            Storage = storage
        });
        AddModule(new StructureSpriteCullModule()); 
        AddModule(new SpriteOrbitModule()); 
        AddState(new InBuildingState());
    }
    

    public void OnActionSecondary()
    {
        if (IsCurrentState<DefaultState>())
            SetState<InBuildingState>();
        else 
            SetState<DefaultState>();
    }
}

public class InBuildingState : State
{
    public override void OnEnterState()
    {
        Audio.PlaySFX("text", 0.5f);
        GUIMain.Building.Storage = ((ContainerInfo)Info).Storage;
        GUIMain.RefreshStorage(); 
        GUIMain.Show(true);
        GUIMain.Building.Show(true, !GUIMain.Showing);
    }

    public override void OnUpdateState()
    {
        if (!GUIMain.Showing || Utility.SquaredDistance(Game.Player.transform.position, Machine.transform.position) > 49) { //walk away from npc
            Machine.SetState<DefaultState>();
        }
    }

    public override void OnExitState()
    {
        GUIMain.Building.Show(false);
    }
}