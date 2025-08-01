using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemMachine : EntityMachine
{
    public bool pickUp = true;
    private bool _wasInRange;
    
    public override void OnInitialize()
    { 
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        AddState(new ItemState());
        AddModule(new ItemPhysicModule()); 
        AddModule(new SpriteCullModule(spriteRenderer)); 
        AddModule(new SpriteOrbitModule(spriteRenderer)); 
    }

    public override void OnUpdate()
    {
        if (Vector3.Distance(transform.position, Game.Player.transform.position) <= 0.8f)
        { 
            if (pickUp)
            {
                Audio.PlaySFX(Game.PickUpSound, 0.4f);
                Inventory.AddItem(GetEntityData().stringID, 1);
                Delete();
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
    SpriteRenderer _sprite;

    public override void OnEnterState()
    {
        _itemPhysicModule = Machine.GetModule<ItemPhysicModule>();
        _itemPhysicModule.PopItem();
        _sprite = Machine.transform.GetComponent<SpriteRenderer>();
    }

    public override void OnUpdateState()
    { 
        if (Machine.transform.position.y < -5)
            ((EntityMachine)Machine).Delete();
        else if (_sprite.isVisible && MapLoad.ActiveChunks.ContainsKey(World.GetChunkCoordinate(Machine.transform.position))) 
            _itemPhysicModule.HandlePhysicsUpdate();
    }
}