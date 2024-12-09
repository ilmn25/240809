using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemStateMachine : EntityStateMachine
{
    private ItemPhysicModule _itemPhysicModule;
    public bool pickUp = true;
    private bool wasInRange = false;
    
    protected override void OnAwake()
    {
        _itemPhysicModule = GetComponent<ItemPhysicModule>();
        
        AddState(new ItemIdle(_itemPhysicModule), true);
    }

    protected override void LogicUpdate()
    {
        if (Vector3.Distance(transform.position, Game.Player.transform.position) <= 0.8f)
        { 
            if (pickUp)
            {
                AudioSingleton.PlaySFX(Game.PickUpSound);
                InventorySingleton.AddItem(GetEntityData().stringID, 1);
                WipeEntity();
            } 
            wasInRange = true;
        }
        else if (wasInRange)
        {
            pickUp = true;
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