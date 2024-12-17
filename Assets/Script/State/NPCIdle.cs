using UnityEngine;

class NPCIdle : State {
    NPCMovementModule _npcMovementModule;
    NPCAnimationModule _npcAnimationModule;  
    SpriteRenderer _sprite;
    public override void OnInitialize()
    {
        _npcMovementModule = Machine.GetModule<NPCMovementModule>();
        _npcAnimationModule = Machine.GetModule<NPCAnimationModule>();
        _sprite = Machine.transform.Find("sprite").GetComponent<SpriteRenderer>();
    }

    public override void OnUpdateState() {
        if (_sprite.isVisible && MapLoadSingleton.Instance._activeChunks.ContainsKey(World.GetChunkCoordinate(Machine.transform.position)))
        {
            _npcMovementModule.HandleMovementUpdate(Vector3.zero);
            _npcAnimationModule.HandleAnimationUpdate();
        }
    }
}