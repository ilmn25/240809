using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// Unified, immediate storage sync.  Removes storage from the 25 ms batch loops
/// (EntitySync / PlayerSync) so that chest, player-inventory, and crafting-pool
/// changes are broadcast the instant the host receives them.
/// </summary>
public struct StorageSyncMessage : NetworkMessage
{
    public string entityUid;
    public byte[] storageData;

    // Secondary slot for atomic two‑way transfers (player↔player, player↔chest)
    public string entityUid2;
    public byte[] storageData2;

    // Crafting‑station pending queue (serialised List<ID>)
    public byte[] pendingData;
}

public static class StorageSync
{
    public static void RegisterHandlers()
    {
        // Client → Host
        NetworkServer.ReplaceHandler<StorageSyncMessage>(OnServerStorageSync, false);
        // Host → Client (immediate broadcast)
        NetworkClient.ReplaceHandler<StorageSyncMessage>(OnClientStorageSync, false);
        // Auto-sync any storage modification via Storage.OnChanged
        Storage.OnChanged += OnStorageChanged;
    }

    // ── Public API ────────────────────────────────────────────────

    /// <summary>Send a single storage update.  On the host, broadcasts to all clients.</summary>
    public static void Send(string entityUid, Storage storage)
    {
        if (string.IsNullOrEmpty(entityUid) || storage?.List == null) return;
        var msg = new StorageSyncMessage
        {
            entityUid = entityUid,
            storageData = Helper.SerializeObject(storage.List)
        };
        if (NetworkServer.active)
        {
            // Host / dedicated server: apply + broadcast immediately
            ProcessServerMessage(msg);
        }
        else if (NetworkClient.isConnected)
        {
            NetworkClient.Send(msg);
        }
    }

    /// <summary>Send an atomic two‑storage transfer.</summary>
    public static void SendTransfer(string srcUid, Storage src, string tgtUid, Storage tgt)
    {
        if (string.IsNullOrEmpty(srcUid) || src?.List == null) return;
        if (string.IsNullOrEmpty(tgtUid) || tgt?.List == null) return;
        var msg = new StorageSyncMessage
        {
            entityUid = srcUid,
            storageData = Helper.SerializeObject(src.List),
            entityUid2 = tgtUid,
            storageData2 = Helper.SerializeObject(tgt.List)
        };
        if (NetworkServer.active)
        {
            ProcessServerMessage(msg);
        }
        else if (NetworkClient.isConnected)
        {
            NetworkClient.Send(msg);
        }
    }

    /// <summary>Send a crafting‑station update (storage + pending queue).</summary>
    public static void SendCraftUpdate(string entityUid, Storage storage, List<ID> pending)
    {
        if (string.IsNullOrEmpty(entityUid) || storage?.List == null) return;
        var msg = new StorageSyncMessage
        {
            entityUid = entityUid,
            storageData = Helper.SerializeObject(storage.List),
            pendingData = pending != null ? Helper.SerializeObject(pending) : null
        };
        if (NetworkServer.active)
        {
            ProcessServerMessage(msg);
        }
        else if (NetworkClient.isConnected)
        {
            NetworkClient.Send(msg);
        }
    }

    /// <summary>Subscribed to Storage.OnChanged — auto-syncs any storage modification.</summary>
    private static void OnStorageChanged(Storage storage)
    {
        if (storage?.info == null) return;
        Send(storage.info.uid, storage);
    }

    /// <summary>Client-authoritative: just relay to all clients — no server-side apply.</summary>
    private static void ProcessServerMessage(StorageSyncMessage msg)
    {
        if (!NetworkServer.active) return;
        NetworkServer.SendToAll(msg);
    }

    private static void OnServerStorageSync(NetworkConnectionToClient conn, StorageSyncMessage msg)
    {
        ProcessServerMessage(msg);
    }

    // ── Client handler ────────────────────────────────────────────

    private static void OnClientStorageSync(StorageSyncMessage msg)
    {
        // Apply primary
        ApplyOne(msg.entityUid, msg.storageData);
        // Apply secondary
        ApplyOne(msg.entityUid2, msg.storageData2);

        // Apply crafting pending queue
        if (!string.IsNullOrEmpty(msg.entityUid) && msg.pendingData != null)
        {
            var info = ResolveInfo(msg.entityUid);
            if (info is CraftInfo craft)
            {
                var pending = Helper.DeserializeObject<List<ID>>(msg.pendingData);
                if (pending != null) { craft.Pending.Clear(); craft.Pending.AddRange(pending); }
            }
        }

        // Refresh UI — guard against uninitialized state during early connection
        if (Main.PlayerInfo != null)
        {
            Inventory.RefreshInventory();
            GUIBar.Update();
        }
        if (GUIMain.StorageInv != null) GUIMain.RefreshStorage();
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static void ApplyOne(string entityUid, byte[] storageData)
    {
        if (string.IsNullOrEmpty(entityUid) || storageData == null) return;

        var info = ResolveInfo(entityUid);
        if (info == null) return;

        Storage storage = null;
        if (info is ContainerInfo ci) storage = ci.Storage;
        else if (info is PlayerInfo pi) storage = pi.Storage;
        else if (info is CraftInfo craft) storage = craft.GetStoragePool();

        if (storage == null) return;

        var list = Helper.DeserializeObject<List<ItemSlot>>(storageData);
        if (list != null) storage.List = list;
    }



    private static Info ResolveInfo(string uid)
    {
        // EntitySync.InfoMap is populated on the client by batch handler;
        // on the host it contains only entities added by PlayerSync.
        if (EntitySync.InfoMap.TryGetValue(uid, out Info info))
            return info;

        // Fallback: global Info dictionary — populated by EntityMachine.Initialize()
        // on both host and client, so it includes all entities (including static ones
        // like crafting stations that aren't in EntitySync.InfoMap on the host).
        if (Info.Dictionary.TryGetValue(uid, out info))
            return info;

        // Fallback: players (host server doesn't keep them in EntitySync.InfoMap)
        if (Save.Inst != null)
            for (int i = 0; i < Save.Inst.players.Count; i++)
                if (Save.Inst.players[i].uid == uid)
                    return Save.Inst.players[i];

        return null;
    }

    public static void Clear()
    {
        Storage.OnChanged -= OnStorageChanged;
    }
}
