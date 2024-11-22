using UnityEngine;
class NPCIdle : EntityState {
    NPCMovementInst _npcMovementInst;
    NPCAnimationInst _npcAnimationInst; 

    public NPCIdle(NPCMovementInst npcMovementInst, NPCAnimationInst npcAnimationInst)
    {
        _npcMovementInst = npcMovementInst;
        _npcAnimationInst = npcAnimationInst;
    }
 
    public override void OnEnterState() {}
    public override void StateUpdate() {
        _npcMovementInst.HandleMovementUpdate();
        _npcAnimationInst.HandleAnimationUpdate();
    }
    public override void OnExitState() {}
}

class NPCChase : EntityState {
    NPCMovementInst _npcMovementInst;
    NPCPathFindInst _npcPathFindInst;
    NPCAnimationInst _npcAnimationInst; 
    SpriteRenderer _sprite;

    public NPCChase(NPCMovementInst npcMovementInst, NPCPathFindInst npcPathFindInst, 
        NPCAnimationInst npcAnimationInst, SpriteRenderer sprite)
    {
        _npcMovementInst = npcMovementInst;
        _npcAnimationInst = npcAnimationInst;
        _npcPathFindInst = npcPathFindInst;
        _sprite = sprite; 
    }

    public override void OnEnterState()
    {
        _npcPathFindInst.SetTarget(Game.Player);
    }
    public override void StateUpdate() {
        if (_sprite.isVisible)
        {
            _npcMovementInst.SetDirection(_npcPathFindInst.HandlePathFindActive());
            // _entityMovementHandler.HandleMovementUpdateTest();
            _npcMovementInst.HandleMovementUpdate();
            _npcAnimationInst.HandleAnimationUpdate();
        }
        else
        {  
            _npcPathFindInst.HandlePathFindPassive(); 
        }
    }
    public override void OnExitState(){}
}