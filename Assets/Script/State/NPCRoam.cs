using UnityEngine;

class NPCRoam : State {
    NPCMovementModule _npcMovementModule;
    PathingModule _pathingModule;
    NPCAnimationModule _npcAnimationModule; 
    SpriteRenderer _sprite;

    public override void OnInitialize()
    {
        _npcMovementModule = Machine.GetModule<NPCMovementModule>();
        _pathingModule = Machine.GetModule<PathingModule>();
        _npcAnimationModule = Machine.GetModule<NPCAnimationModule>();
        _sprite = Machine.transform.Find("sprite").GetComponent<SpriteRenderer>();
    }

    public override void OnEnterState()
    {
        _pathingModule.SetTarget(null);
    }
    
    public override void OnUpdateState() {
        if (_sprite.isVisible && MapLoadSingleton.Instance._activeChunks.ContainsKey(World.GetChunkCoordinate(Machine.transform.position)))
        {
            _npcMovementModule.HandleMovementUpdate(_pathingModule.GetNextDirection());
            _npcAnimationModule.HandleAnimationUpdate();
        }
        else
        {  
            _pathingModule.PassivePathFollow(_npcMovementModule.SPEED_WALK); 
        }
    }
}