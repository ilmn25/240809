
using UnityEngine;

public class PlayerState : State
{
    public override void OnEnterState()
    {
        AddState(new PlayerActive(), true); 
    }

    public override void StateUpdate()
    {
        if (Root.transform.position.y < -50)
        {
            MapCullSingleton.Instance.ForceRevertMesh(); 
            Root.transform.position = new Vector3(Game.Player.transform.position.x , WorldSingleton.World.Bounds.y + 40, Game.Player.transform.position.z);
        }
        
        if (Input.GetKeyDown(KeyCode.O)) 
            EntitySingleton.SpawnPrefab("chito", Vector3Int.FloorToInt(Root.transform.position + Vector3.up));
    }
} 
class PlayerActive : State { 
    public override void StateUpdate() { 
        PlayerMovementSingleton.Instance.HandleMovementUpdate();
        PlayerAnimationSingleton.Instance.HandleAnimationUpdate(); 
        PlayerChunkEditSingleton.Instance.HandleChunkEditInput(); 
        InventorySingleton.Instance.HandleInventoryUpdate();
        PlayerStatusSingleton.Instance.HandleStatusUpdate();
    }
}

