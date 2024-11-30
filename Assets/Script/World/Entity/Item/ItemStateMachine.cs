using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemStateMachine : EntityStateMachine
{
    private ItemPhysicModule _itemPhysicModule;
    protected override void OnAwake()
    {
        _itemPhysicModule = GetComponent<ItemPhysicModule>();
        
        AddState(new ItemIdle(_itemPhysicModule), true);
    }

    protected override void LogicUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            float playerDistance = Vector3.Distance(transform.position, Game.Player.transform.position);
        
            if (playerDistance <= 0.8f)
            {
                PlayerInventorySingleton.AddItem(GetEntityData().ID, 1);
                WipeEntity();
            }
        } 
    }
}

public class ItemIdle : EntityState
{
    private ItemPhysicModule _itemPhysicModule;
    public ItemIdle(ItemPhysicModule itemPhysicModule)
    {
        _itemPhysicModule = itemPhysicModule;
    }

    public override void OnEnterState()
    {
        _itemPhysicModule.PopItem();
    }

    public override void StateUpdate()
    {
 
        if (StateMachine.transform.position.y < -5) 
        {            
            StateMachine.WipeEntity();
        } else _itemPhysicModule.HandlePhysicsUpdate();
    }
}