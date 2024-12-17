using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemMachine : EntityMachine
{
    public bool pickUp = true;
    private bool _wasInRange;
    
    public override void OnInitialize()
    { 
        AddState(new ItemState());
        AddModule(new ItemPhysicModule());
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        AddModule(new SpriteCullModule(spriteRenderer)); 
        AddModule(new SpriteOrbitModule(spriteRenderer)); 
    }

    public override void OnUpdate()
    {
        if (Vector3.Distance(transform.position, Game.Player.transform.position) <= 0.8f)
        { 
            if (pickUp)
            {
                AudioSingleton.PlaySFX(Game.PickUpSound);
                InventorySingleton.AddItem(GetEntityData().stringID, 1);
                WipeEntity();
            } 
            _wasInRange = true;
        }
        else if (_wasInRange)
        {
            pickUp = true;
        }
    }
}

public class ItemState : State
{
    private ItemPhysicModule _itemPhysicModule; 

    public override void OnEnterState()
    {
        _itemPhysicModule = Machine.GetModule<ItemPhysicModule>();
        _itemPhysicModule.PopItem();
    }

    public override void OnUpdateState()
    { 
        if (Machine.transform.position.y < -5)
        { 
            ((EntityMachine)Machine).WipeEntity();
        } else _itemPhysicModule.HandlePhysicsUpdate();
    }
}