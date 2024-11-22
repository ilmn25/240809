using System;
using UnityEngine;

public abstract class State
{
    public String name;
    public String[] tag;

    protected State(string name, string[] tag = null)
    {
        this.name = name;
        this.tag = tag;
    }

    public abstract void OnEnterState();
    public abstract void StateUpdate();
    public abstract void OnExitState();
}

class NPCIdle : State {
    NPCMovementInst _npcMovementInst;
    NPCAnimationInst _npcAnimationInst; 

    public NPCIdle(NPCMovementInst npcMovementInst, NPCAnimationInst npcAnimationInst) : base("idle")
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

class NPCChase : State {
    NPCMovementInst _npcMovementInst;
    NPCPathFindInst _npcPathFindInst;
    NPCAnimationInst _npcAnimationInst; 
    SpriteRenderer _sprite;

    public NPCChase(NPCMovementInst npcMovementInst, NPCPathFindInst npcPathFindInst, 
        NPCAnimationInst npcAnimationInst, SpriteRenderer sprite) : base("chase")
    {
        _npcMovementInst = npcMovementInst;
        _npcAnimationInst = npcAnimationInst;
        _npcPathFindInst = npcPathFindInst;
        _sprite = sprite; 
    }
 
    public override void OnEnterState() {}
    public override void StateUpdate() {
        if (_sprite.isVisible)
        {
            _npcPathFindInst.HandlePathFindActive();
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