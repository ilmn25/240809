using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemMachine : EntityMachine, IActionSecondaryPickUp
{    
    
    public static Info CreateInfo()
    {
        return new ItemInfo();
    }
    private ItemInfo ItemInfo => (ItemInfo)Info; 

    private static readonly Collider[] CollisionArray = new Collider[40];
    private static int _collisionCount; 
    private float _deltaTime;
    private const float Gravity = 35;
    private const float BounceFactor = 0.3f;
    private const float CollisionRange = 0.3f; 
    public override void OnStart()
    {
        if (ItemInfo.StackOnSpawn)
        {
            ItemInfo nearbyItem;
            _collisionCount = Physics.OverlapSphereNonAlloc(transform.position, 2, CollisionArray, Game.MaskEntity);
            for (int i = 0; i < _collisionCount; i++)
            { 
                Collider col = CollisionArray[i];

                if (col.gameObject == gameObject)
                    continue;

                if (col.gameObject.name == "ItemPrefab")
                {
                    nearbyItem = col.GetComponent<ItemMachine>().ItemInfo;
                    if (nearbyItem.item.isSame(ItemInfo.item))
                    {
                        ItemInfo.item.Add(nearbyItem.item);
                        if (nearbyItem.item.isEmpty()) nearbyItem.Destroy();
                        if (ItemInfo.item.isFull()) break;
                    }
                }
            }
        } 
        
        ItemInfo.SpriteRenderer.sprite = Cache.LoadSprite("Sprite/" + ItemInfo.item.ID);
        AddModule(new ItemSpriteCullModule()); 
        transform.rotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
        transform.localScale = Vector3.one * ItemInfo.item.Info.Scale;
        if (ItemInfo.Velocity == default) 
            ItemInfo.Velocity = new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3));  
    }

    public override void OnUpdate()
    { 
        if (!ItemInfo.IsInRenderRange)  return;
        _deltaTime = Helper.GetDeltaTime();

        if (!IsMovable(transform.position))
        {
            transform.position += new Vector3(0, 5, 0) * _deltaTime;
            return;
        }

        ItemInfo.Velocity += Gravity * _deltaTime * Vector3.down;
        ItemInfo.Velocity.y = Mathf.Max(ItemInfo.Velocity.y, -Gravity); 
        Vector3 newPosition = transform.position + ItemInfo.Velocity * _deltaTime;
        
        if (IsMovable(newPosition))
        {
            transform.position = newPosition;
        }
        else
        { 
            ItemInfo.Velocity = -ItemInfo.Velocity * BounceFactor;
        }

        return;

        bool IsMovable(Vector3 pos)
        {  
            _collisionCount = Physics.OverlapSphereNonAlloc(pos + new Vector3(0,0.2f,0), CollisionRange, CollisionArray, Game.MaskStatic);

            return !(_collisionCount > 0);
        } 
    }
}
 