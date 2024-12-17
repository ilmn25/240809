
using UnityEngine;

public class PlayerState : State
{
    public override void OnEnterState()
    {
        AddState(new PlayerActive(), true); 
    }

    public override void OnUpdateState()
    {
        if (Machine.transform.position.y < -50)
        {
            MapCullSingleton.Instance.ForceRevertMesh(); 
            Machine.transform.position = new Vector3(Game.Player.transform.position.x , World.world.Bounds.y + 40, Game.Player.transform.position.z);
        }
        
        if (Input.GetKeyDown(KeyCode.O)) 
            EntitySingleton.SpawnPrefab("chito", Vector3Int.FloorToInt(Machine.transform.position + Vector3.up));
        if (Input.GetKeyDown(KeyCode.M)) 
            EntitySingleton.SpawnPrefab("megumin", Vector3Int.FloorToInt(Machine.transform.position + Vector3.up));
        if (Input.GetKeyDown(KeyCode.L)) 
            EntitySingleton.SpawnPrefab("snare_flea", Vector3Int.FloorToInt(Machine.transform.position + Vector3.up));
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

