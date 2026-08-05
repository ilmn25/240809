using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>Networking for ground items: one-shot spawns, position updates, and destroys.</summary>
public static class ItemSync
{
    public struct BatchItemInfoMessage : NetworkMessage
    {
        public string[] uids;
        /// <summary>Item ID for spawns; 0 for position updates/destroys.</summary>
        public int[] ids;
        public Vector3[] positions;
        public bool[] destroyed;
        public int[] itemAmounts;
    }

    public static float BroadcastInterval = 0.2f;

    private class PendingSpawn { public Info info; }
    private class PendingUnload { public string uid; public int id; public Vector3 pos; }
    private static readonly List<PendingSpawn> _pendingSpawns = new List<PendingSpawn>();
    private static readonly List<PendingUnload> _pendingUnloads = new List<PendingUnload>();

    private static readonly List<string> _batchUids = new List<string>();
    private static readonly List<int> _batchIds = new List<int>();
    private static readonly List<Vector3> _batchPositions = new List<Vector3>();
    private static readonly List<bool> _batchDestroyed = new List<bool>();
    private static readonly List<int> _batchAmounts = new List<int>();

    public static void RegisterHandlers()
    {
        NetworkClient.ReplaceHandler<BatchItemInfoMessage>(OnBatchInfoReceived, false);
        if (Application.isPlaying)
            _ = new CoroutineTask(BatchLoop());
    }

    /// <summary>Queue a one-shot spawn broadcast (drop, loot, chunk load).</summary>
    public static void BroadcastSpawn(Info info)
    {
        if (!Helper.IsHost() || !NetworkServer.active || info == null || info.Machine == null) return;
        _pendingSpawns.Add(new PendingSpawn { info = info });
    }

    /// <summary>Queue a destroy broadcast (pickup, out of range, world save).</summary>
    public static void BroadcastUnload(Info info)
    {
        if (!Helper.IsHost() || !NetworkServer.active || info == null) return;
        if (info is ItemInfo itemInfo)
            _pendingUnloads.Add(new PendingUnload { uid = info.uid, id = (int)itemInfo.item.ID, pos = info.position });
    }

    /// <summary>Send all currently active items to a single connection.</summary>
    public static void SendActiveItems(NetworkConnectionToClient conn)
    {
        if (!NetworkServer.active || conn == null) return;

        _batchUids.Clear(); _batchIds.Clear(); _batchPositions.Clear();
        _batchDestroyed.Clear(); _batchAmounts.Clear();

        foreach (var em in EntityItemLoad.ActiveEntities)
        {
            if (em == null || em.Info is not ItemInfo itemInfo) continue;
            _batchUids.Add(itemInfo.uid);
            _batchIds.Add((int)itemInfo.item.ID);
            _batchPositions.Add(itemInfo.position);
            _batchDestroyed.Add(false);
            _batchAmounts.Add(itemInfo.item.Stack);
        }

        if (_batchUids.Count == 0) return;
        conn.Send(new BatchItemInfoMessage
        {
            uids = _batchUids.ToArray(),
            ids = _batchIds.ToArray(),
            positions = _batchPositions.ToArray(),
            destroyed = _batchDestroyed.ToArray(),
            itemAmounts = _batchAmounts.ToArray()
        });
    }

    /// <summary>Client-side cleanup on disconnect: unload synced items and drop their InfoMap entries.</summary>
    public static void Clear()
    {
        if (Helper.IsHost()) return;
        foreach (var em in new List<EntityMachine>(EntityItemLoad.ActiveEntities))
        {
            if (em?.Info == null) continue;
            EntitySync.InfoMap.Remove(em.Info.uid);
            em.Unload();
        }
    }

    private static IEnumerator BatchLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(BroadcastInterval);
            if (!NetworkServer.active) continue;
            SendBatch();
        }
    }

    /// <summary>Flush item spawns, position updates, and unloads.</summary>
    private static void SendBatch()
    {
        if (!NetworkServer.active) return;
        _batchUids.Clear(); _batchIds.Clear(); _batchPositions.Clear();
        _batchDestroyed.Clear(); _batchAmounts.Clear();

        void AddItem(Info info, bool destroyed)
        {
            if (info is not ItemInfo itemInfo) return;
            _batchUids.Add(info.uid);
            _batchIds.Add((int)itemInfo.item.ID);
            _batchPositions.Add(info.position);
            _batchDestroyed.Add(destroyed);
            _batchAmounts.Add(destroyed ? 0 : itemInfo.item.Stack);
        }

        // One-shot spawns
        foreach (var ps in _pendingSpawns)
            if (ps.info?.Machine != null) AddItem(ps.info, false);
        _pendingSpawns.Clear();

        // Dirty position updates
        foreach (var em in EntityItemLoad.ActiveEntities)
        {
            if (em == null || em.Info is not ItemInfo itemInfo) continue;
            if (!itemInfo.PositionDirty) continue;
            AddItem(itemInfo, false);
            itemInfo.MarkPositionSynced();
        }

        // Unloads
        foreach (var pu in _pendingUnloads)
        {
            _batchUids.Add(pu.uid);
            _batchIds.Add(pu.id);
            _batchPositions.Add(pu.pos);
            _batchDestroyed.Add(true);
            _batchAmounts.Add(0);
        }
        _pendingUnloads.Clear();

        if (_batchUids.Count == 0) return;
        NetworkServer.SendToAll(new BatchItemInfoMessage
        {
            uids = _batchUids.ToArray(),
            ids = _batchIds.ToArray(),
            positions = _batchPositions.ToArray(),
            destroyed = _batchDestroyed.ToArray(),
            itemAmounts = _batchAmounts.ToArray()
        });
    }

    private static void OnBatchInfoReceived(BatchItemInfoMessage message)
    {
        if (Helper.IsHost()) return;
        for (int i = 0; i < message.uids.Length; i++)
        {
            string uid = message.uids[i];
            if (string.IsNullOrEmpty(uid)) continue;
            bool destroyed = message.destroyed[i];

            // New item
            if (!EntitySync.InfoMap.TryGetValue(uid, out Info info))
            {
                int itemId = message.ids[i];
                info = Entity.CreateInfo((ID)itemId, message.positions[i]);
                info.uid = uid;
                info.position = message.positions[i];
                if (info is ItemInfo itemInfo)
                {
                    if (itemInfo.item == null || itemInfo.item.ID == ID.Null)
                        itemInfo.item = new ItemSlot((ID)itemId);
                    if (message.itemAmounts != null && i < message.itemAmounts.Length && message.itemAmounts[i] > 1)
                        itemInfo.item.Stack = message.itemAmounts[i];
                }
                EntitySync.InfoMap[uid] = info;
                Entity.SpawnFromInfo(info, true);
                continue;
            }

            // Destroyed
            if (destroyed)
            {
                if (info.Machine != null)
                    ((EntityMachine)info.Machine).Unload();
                EntitySync.InfoMap.Remove(uid);
                continue;
            }

            // Position update
            if (info is ItemInfo && info.Machine != null)
            {
                info.position = message.positions[i];
                info.Machine.transform.position = info.position;
            }
        }
    }
}
