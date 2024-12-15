using UnityEngine;

class NPCRoam : State {
    NPCMovementModule _npcMovementModule;
    NPCPathFindAbstract _npcPathFindAbstract;
    NPCAnimationModule _npcAnimationModule; 
    SpriteRenderer _sprite;

    public override void OnInitialize()
    {
        _npcMovementModule = Machine.GetModule<NPCMovementModule>();
        _npcPathFindAbstract = Machine.GetModule<NPCPathFindAbstract>();
        _npcAnimationModule = Machine.GetModule<NPCAnimationModule>();
        _sprite = Machine.transform.Find("sprite").GetComponent<SpriteRenderer>();
    }

    public override void OnEnterState()
    {
        _npcPathFindAbstract.SetTarget(null);
    }
    
    public override void OnUpdateState() {
        if (_sprite.isVisible)
        {
            _npcMovementModule.SetDirection(_npcPathFindAbstract.GetNextDirection(_npcMovementModule.IsGrounded()));
            _npcMovementModule.HandleMovementUpdate();
            _npcAnimationModule.HandleAnimationUpdate();
        }
        else
        {  
            _npcPathFindAbstract.PassivePathFollow(_npcMovementModule.SPEED_WALK); 
        }
    }
}