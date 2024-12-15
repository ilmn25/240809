using UnityEngine;

class NPCChase : State {
    NPCMovementModule _npcMovementModule;
    NPCPathFindAbstract _npcPathFindAbstract;
    NPCAnimationModule _npcAnimationModule; 
    SpriteRenderer _sprite;

    public NPCChase(NPCMovementModule npcMovementModule, NPCPathFindAbstract npcPathFindAbstract, 
        NPCAnimationModule npcAnimationModule, SpriteRenderer sprite)
    {
        _npcMovementModule = npcMovementModule;
        _npcAnimationModule = npcAnimationModule;
        _npcPathFindAbstract = npcPathFindAbstract;
        _sprite = sprite; 
    }

    public override void OnEnterState()
    {
        _npcPathFindAbstract.SetTarget(Game.Player.transform);
    }
    
    public override void OnUpdateState() {
        if (_sprite.isVisible)
        {
            _npcMovementModule.SetDirection(_npcPathFindAbstract.HandlePathFindActive(_npcMovementModule.IsGrounded()));
            _npcMovementModule.HandleMovementUpdate();
            _npcAnimationModule.HandleAnimationUpdate();
        }
        else
        {  
            _npcPathFindAbstract.HandlePathFindPassive(_npcMovementModule.SPEED_WALK); 
        }
    }
}