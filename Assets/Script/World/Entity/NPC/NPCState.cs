using UnityEngine;
class NPCIdle : EntityState {
    NPCMovementModule _npcMovementModule;
    NPCAnimationModule _npcAnimationModule; 

    public NPCIdle(NPCMovementModule npcMovementModule, NPCAnimationModule npcAnimationModule)
    {
        _npcMovementModule = npcMovementModule;
        _npcAnimationModule = npcAnimationModule;
    }
 
    public override void StateUpdate() {
        _npcMovementModule.HandleMovementUpdate();
        _npcAnimationModule.HandleAnimationUpdate();
    }
}

class NPCRoam : EntityState {
    NPCMovementModule _npcMovementModule;
    NPCPathFindAbstract _npcPathFindAbstract;
    NPCAnimationModule _npcAnimationModule; 
    SpriteRenderer _sprite;

    public NPCRoam(NPCMovementModule npcMovementModule, NPCPathFindAbstract npcPathFindAbstract, 
        NPCAnimationModule npcAnimationModule, SpriteRenderer sprite)
    {
        _npcMovementModule = npcMovementModule;
        _npcAnimationModule = npcAnimationModule;
        _npcPathFindAbstract = npcPathFindAbstract;
        _sprite = sprite; 
    }

    public override void OnEnterState()
    {
        _npcPathFindAbstract.SetTarget(null);
    }
    
    public override void StateUpdate() {
        if (_sprite.isVisible)
        {
            _npcMovementModule.SetDirection(_npcPathFindAbstract.HandlePathFindRandom(_npcMovementModule.IsGrounded()));
            _npcMovementModule.HandleMovementUpdate(true);
            _npcAnimationModule.HandleAnimationUpdate();
        }
        else
        {  
            _npcPathFindAbstract.HandlePathFindPassive(_npcMovementModule.SPEED_WALK); 
        }
    }
}

class NPCChase : EntityState {
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
    
    public override void StateUpdate() {
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