using System;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class ItemInfo : Info
{
    public ItemSlot item;
    public int despawn;
    [NonSerialized] public bool StackOnSpawn = false;
    [NonSerialized] public Vector3 Velocity;
    [NonSerialized] public SpriteRenderer SpriteRenderer; 

    public override void Initialize()
    { 
        SpriteRenderer = Machine.transform.GetComponent<SpriteRenderer>();
    }

    public override void Update()
    {
        if (despawn > 0)
        {
            despawn--;
            if (despawn == 0) Destroy();
        }
        
        if (Machine)
        {
            position = Machine.transform.position;
            if (Helper.IsHost())
            {
                // On the host: entity is in render range if ANY player is close enough.
                // Prevents items from being simulated with abstract movement near remote clients.
                Vector3Int chunkCoord = World.GetChunkCoordinate(position);
                IsInRenderRange = Scene.AnyPlayerInChunkRange(chunkCoord, Scene.RenderDistance);
            }
            else
            {
                IsInRenderRange = SpriteRenderer.isVisible &&
                                  MapLoad.ActiveChunks.ContainsKey(World.GetChunkCoordinate(position));
            }
        }
    }

    public void OnActionSecondary(Info info)
    {        
        if (Vector3.Distance(position, info.Machine.transform.position) < 3f) 
        { 
            Audio.PlaySFX(SfxID.Item);
            ((PlayerInfo)info).Storage.AddItem(item);
            Inventory.RefreshInventory();
            Destroy();
        }
    }

    public override string ToString()
    {
        return string.Empty;
    }
}