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
    /// <summary>Host-side: true while the item's position has changed since the last
    /// item-batch broadcast. Cleared by MarkPositionSynced(). Never set on clients.</summary>
    [NonSerialized] public bool PositionDirty = false;
    [NonSerialized] private Vector3 _lastSyncedPosition;
    [NonSerialized] private float _despawnTimer;

    public override void Initialize()
    { 
        SpriteRenderer = Machine.transform.GetComponent<SpriteRenderer>();
        _lastSyncedPosition = position;
        // Seed the timed-despawn timer from the item's definition.
        _despawnTimer = item.Info.DespawnTime;
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
            Vector3 newPosition = Machine.transform.position;
            if (Vector3.Distance(newPosition, _lastSyncedPosition) > 0.1f)
                PositionDirty = true;
            position = newPosition;
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

        // Timed despawn (ash, etc.) — host-authoritative.
        if (Helper.IsHost() && _despawnTimer > 0f)
        {
            _despawnTimer -= Helper.GetDeltaTime();
            if (_despawnTimer <= 0f)
            {
                Destroy();
                return;
            }
        }
    }

    /// <summary>Host: mark this item as sent to clients, so it stops being re-broadcast.</summary>
    public void MarkPositionSynced()
    {
        _lastSyncedPosition = position;
        PositionDirty = false;
    }

    public void OnActionSecondary(Info info)
    {        
        // Non-pickupable items (blood) can't be collected.
        if (!item.Info.Pickupable) return;
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