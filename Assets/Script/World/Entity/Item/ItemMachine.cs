using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemMachine : EntityMachine
{
    private ItemPhysicModule _itemPhysicModule;
    public bool pickUp = true;
    private bool wasInRange = false;
    
    public override void OnAwake()
    {
        _itemPhysicModule = GetComponent<ItemPhysicModule>();

        State = new ItemState(_itemPhysicModule);
    }

    public override void OnUpdate()
    {
        if (Vector3.Distance(transform.position, Game.Player.transform.position) <= 0.8f)
        { 
            if (pickUp)
            {
                AudioSingleton.PlaySFX(Game.PickUpSound);
                InventorySingleton.Instance.AddItem(GetEntityData().stringID, 1);
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

public class ItemState : State
{
    private ItemPhysicModule _itemPhysicModule;
    public ItemState(ItemPhysicModule itemPhysicModule)
    {
        _itemPhysicModule = itemPhysicModule;
    }

    public override void OnEnterState()
    {
        _itemPhysicModule.PopItem();
    }

    public override void StateUpdate()
    { 
        if (Root.transform.position.y < -5)
        { 
            ((EntityMachine)Root).WipeEntity();
        } else _itemPhysicModule.HandlePhysicsUpdate();
    }
}