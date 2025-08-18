using UnityEngine;

public class ChestMachine : StructureMachine, IActionSecondaryInteract, IActionPrimaryResource
{
    public static Info CreateInfo()
    {
        Storage storage = new Storage(27);
        Loot.Gettable("chest").AddToContainer(storage);
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

public class InContainerState : State
{
    public Storage Storage;
    public override void OnEnterState()
    {
        Audio.PlaySFX("text", 0.5f);
        GUIMain.Storage.Storage = Storage;
        GUIMain.RefreshStorage(); 
        GUIMain.Show(true);
        GUIMain.Storage.Show(true, !GUIMain.Showing);
    }

    public override void OnUpdateState()
    {
        if (!GUIMain.Showing || Helper.SquaredDistance(Game.Player.transform.position, Machine.transform.position) > 5*5) { //walk away from npc
            Machine.SetState<DefaultState>();
        }
    }

    public override void OnExitState()
    {
        GUIMain.Storage.Show(false);
    }
}