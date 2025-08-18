using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemMachine : EntityMachine, IActionSecondaryPickUp
{    
    public static Info CreateInfo()
    {
        return new ItemInfo();
    }
    
    private bool _wasInRange;
    private SpriteRenderer _spriteRenderer;
    private ItemPhysicModule _itemPhysicModule; 
    
    public override void OnSetup()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>(); 
    }

    public override void OnStart()
    {
        _spriteRenderer.sprite = Cache.LoadSprite("sprite/" + Info.stringID);
        AddModule(new ItemPhysicModule()); 
        AddModule(new ItemSpriteCullModule()); 
        transform.rotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
        transform.localScale = Vector3.one * Item.GetItem(Info.stringID).Scale;
        _itemPhysicModule = GetModule<ItemPhysicModule>();
        _itemPhysicModule.PopItem(); 
    }

    public override void OnUpdate()
    {  
        if (transform.position.y < -5) Delete();
        else if (_spriteRenderer.isVisible && MapLoad.ActiveChunks.ContainsKey(World.GetChunkCoordinate(transform.position))) 
            _itemPhysicModule.HandlePhysicsUpdate();
    }
 
}
 