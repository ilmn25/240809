using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemStateMachine : EntityStateMachine
{
    private NPCMovementInst _npcMovementInst;
    private NPCPathFindInst _npcPathFindInst;
    private NPCAnimationInst _npcAnimationInst; 
    private SpriteRenderer _sprite;
    protected override void OnAwake()
    {
        _sprite = transform.Find("sprite").GetComponent<SpriteRenderer>();
        _npcMovementInst = GetComponent<NPCMovementInst>();
        _npcPathFindInst = GetComponent<NPCPathFindInst>();
        _npcAnimationInst = GetComponent<NPCAnimationInst>();
        
        AddState(new NPCIdle(_npcMovementInst, _npcAnimationInst), true);
        AddState(new NPCChase(_npcMovementInst, _npcPathFindInst, _npcAnimationInst, _sprite));
    }
 
}