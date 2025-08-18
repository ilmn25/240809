using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemMachine : EntityMachine, IActionSecondaryPickUp
{    
    private static readonly Collider[] CollisionArray = new Collider[40];
    private static int _collisionCount; 
    private bool _wasInRange;
    private SpriteRenderer _spriteRenderer;
    private ItemPhysicModule _itemPhysicModule; 
    public new ItemInfo Info => GetModule<ItemInfo>();

    
    public static Info CreateInfo()
    {
        return new ItemInfo();
    }
     
    
    public override void OnSetup()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>(); 
    }

    public override void OnStart()
    {
        ItemInfo nearbyItem;
        _collisionCount = Physics.OverlapSphereNonAlloc(transform.position, 2, CollisionArray, Game.MaskEntity);
        for (int i = 0; i < _collisionCount; i++)
        { 
            Collider col = CollisionArray[i];

            if (col.gameObject == gameObject)
                continue;

            if (col.gameObject.name == "item")
            {
                nearbyItem = col.GetComponent<ItemMachine>().Info;
                if (nearbyItem.item.isSame(Info.item))
                {
                    Info.item.Add(nearbyItem.item);
                    if (nearbyItem.item.isEmpty()) ((ItemMachine)nearbyItem.Machine).Delete();
                    if (Info.item.isFull()) break;
                }
            }
        }
        
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
 