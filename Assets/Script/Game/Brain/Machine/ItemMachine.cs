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
    // How far an item wedged in a block may be lifted to escape it. Bounded so a
    // solid column can never send it climbing.
    private const int MaxEscapeHeight = 8;
    public override void OnStart()
    {
        if (ItemInfo.StackOnSpawn)
        {
            ItemInfo nearbyItem;
            _collisionCount = Physics.OverlapSphereNonAlloc(transform.position, 2, CollisionArray, Main.MaskEntity);
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

        // An item inside a block (spawned in a wall, block placed on top of it)
        // is lifted once to the first free cell above so it settles on the
        // surface. Pushing up every frame made it climb forever in thick walls.
        if (!IsMovableCell(transform.position))
        {
            if (TryFindFreePosition(transform.position, out Vector3 freePosition))
            {
                transform.position = freePosition;
                ItemInfo.Velocity = Vector3.zero;
            }
            return;
        }
        ItemInfo.Velocity += Gravity * _deltaTime * Vector3.down;
        ItemInfo.Velocity.y = Mathf.Max(ItemInfo.Velocity.y, -Gravity); 
        Vector3 newPosition = transform.position + ItemInfo.Velocity * _deltaTime;
        
        if (IsMovableCell(newPosition))
        {
            transform.position = newPosition;
        }
        else
        { 
            ItemInfo.Velocity = -ItemInfo.Velocity * BounceFactor;
        }
    }

    /// <summary>Lowest position at or above <paramref name="pos"/> where the item
    /// fits, searching only the cell column it stands in. False when the column is
    /// solid up to the world ceiling, in which case the item stays put.</summary>
    private static bool TryFindFreePosition(Vector3 pos, out Vector3 freePosition)
    {
        int x = Mathf.FloorToInt(pos.x);
        int z = Mathf.FloorToInt(pos.z);
        int startY = Mathf.FloorToInt(pos.y - 0.1f);
        for (int y = startY; y <= startY + MaxEscapeHeight; y++)
        {
            if (!World.IsInWorldBounds(new Vector3Int(x, y, z))) break;
            // Resting height above a block face, matching where a falling item stops.
            Vector3 candidate = new Vector3(pos.x, y + 0.1f, pos.z);
            if (!IsMovableCell(candidate)) continue;
            freePosition = candidate;
            return true;
        }
        freePosition = pos;
        return false;
    }

    private static bool IsMovableCell(Vector3 pos)
    {
        // Equivalent to the old Physics.OverlapSphere(center y+0.2, radius 0.3)
        // which covered y from (pos.y-0.1) to (pos.y+0.5). Check all blocks in
        // that vertical range, matching MovementModule's box-footprint pattern.
        int minY = Mathf.FloorToInt(pos.y - 0.1f);
        int maxY = Mathf.FloorToInt(pos.y + 0.5f);
        int cx = Mathf.FloorToInt(pos.x);
        int cz = Mathf.FloorToInt(pos.z);
        for (int y = minY; y <= maxY; y++)
        {
            Vector3Int b = new Vector3Int(cx, y, cz);
            if (World.IsInWorldBounds(b) && NavMap.Get(b) != NavMap.Air)
                return false;
        }
        return true;
    }
}
 