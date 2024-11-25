using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemStateMachine : EntityStateMachine
{
    private ItemPhysicInst _itemPhysicInst;
    protected override void OnAwake()
    {
        _itemPhysicInst = GetComponent<ItemPhysicInst>();
        
        AddState(new ItemIdle(_itemPhysicInst), true);
    }
}

public class ItemIdle : EntityState
{
    private ItemPhysicInst _itemPhysicInst;
    public ItemIdle(ItemPhysicInst itemPhysicInst)
    {
        _itemPhysicInst = itemPhysicInst;
    }

    public override void OnEnterState()
    {
        _itemPhysicInst.PopItem();
    }

    public override void StateUpdate()
    {

        float playerDistance = Vector3.Distance(StateMachine.transform.position, Game.Player.transform.position);
        
        if (playerDistance <= 0.8f)
        {
            PlayerInventoryStatic.AddItem(StateMachine.GetEntityData().ID, 1);
            StateMachine.WipeEntity();
        }
        else if (StateMachine.transform.position.y < -5) 
        {            
            StateMachine.WipeEntity();
        } else _itemPhysicInst.HandlePhysicsUpdate();
    }
}