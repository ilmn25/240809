using UnityEngine;

public class ChestMachine : StructureMachine, IActionSecondary
{
    public override void OnStart()
    {
        Storage storage = new Storage(27);
        Loot.Gettable("chest").AddToContainer(storage);
        AddModule(new ContainerInfo()
        {
            Health = 500,
            Loot = "tree",
            SfxHit = "dig_stone",
            SfxDestroy = "dig_stone",
            Storage = storage
        });
        AddModule(new SpriteCullModule(SpriteRenderer)); 
        AddModule(new SpriteOrbitModule()); 
        AddState(new InContainerState());
    }
    

    public void OnActionSecondary()
    {
        if (IsCurrentState<DefaultState>())
            SetState<InContainerState>();
        else 
            SetState<DefaultState>();
    }
}

public class InContainerState : State
{
    public override void OnEnterState()
    {
        Audio.PlaySFX("text", 0.5f);
        GUIMain.Storage.Storage = ((ContainerInfo)Info).Storage;
        GUIMain.RefreshStorage();
        GUIMain.Storage.Show(true);
        GUIMain.Show(true);
    }

    public override void OnUpdateState()
    {
        if (!GUIMain.Showing || Utility.SquaredDistance(Game.Player.transform.position, Machine.transform.position) > 5*5) { //walk away from npc
            Machine.SetState<DefaultState>();
        }
    }

    public override void OnExitState()
    {
        GUIMain.Storage.Show(false);
    }
}