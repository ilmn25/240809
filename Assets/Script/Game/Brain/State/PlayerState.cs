
using UnityEngine;

public class PlayerState : State
{
    public override void OnEnterState()
    {
        AddState(new PlayerActive(), false); 
        AddState(new StateEmpty(), true); 
    }

    public override void OnUpdateState()
    {
        if (Game.GameState == GameState.Playing)
            SetState<PlayerActive>();
        else
            SetState<StateEmpty>();
        
        if (Machine.transform.position.y < -50)
        {
            MapCull.ForceRevertMesh(); 
            Machine.transform.position = new Vector3(Game.Player.transform.position.x , World.Inst.Bounds.y + 40, Game.Player.transform.position.z);
        }
         
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Space))
            Game.GameState = GameState.Playing; 
        if (Input.GetKeyDown(KeyCode.M)) 
            Entity.SpawnPrefab("megumin", Vector3Int.FloorToInt(Machine.transform.position + Vector3.up));
        if (Input.GetKeyDown(KeyCode.L)) 
            Entity.SpawnPrefab("snare_flea", Vector3Int.FloorToInt(Machine.transform.position + Vector3.up));
    }
}  
class PlayerActive : State {
    PlayerAnimationModule _playerAnimationModule;
    PlayerMovementModule _playerMovementModule;
    
    public override void OnInitialize()
    {
        _playerMovementModule = Machine.GetModule<PlayerMovementModule>();
        _playerAnimationModule = Machine.GetModule<PlayerAnimationModule>();
    }

    public override void OnUpdateState() { 
        _playerMovementModule.HandleMovementUpdate();
        _playerAnimationModule.HandleAnimationUpdate();  
    }
}

