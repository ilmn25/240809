using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public struct BatchEntityInfoMessage : NetworkMessage
{
    public string[] uids;
    public int[] ids;
    public Vector3[] positions;
    public bool[] destroyed;
    /// <summary>Owner connection ID per entity: 0=host, >0=client.</summary>
    public int[] ownerIds;

    // Animation/runtime primitive arrays
    public Vector3[] animDirections;
    public bool[] animIsGrounded;
    public float[] animSpeedCurrent;
    public float[] animSpeedTarget;
    public bool[] animFaceTarget;
    public Vector3[] animTargetScreenDirs;

    // One-shot animation trigger (shortNameHash of current Animator state, 0 = none)
    public int[] animTriggers;
    public float[] animNormalizedTimes;

    // Item stack count (0 = not an item / no stack)
    public int[] itemAmounts;
    // Item durability (0 = default / not an item)
    public int[] itemDurabilities;
}

public static class EntitySync
{
    public static readonly Dictionary<string, Info> InfoMap = new Dictionary<string, Info>();
    public static float BroadcastInterval = 0.025f;

    public static void RegisterHandlers()
    {
        NetworkClient.ReplaceHandler<BatchEntityInfoMessage>(OnBatchEntityInfoMessageReceived, false);
        if (Application.isPlaying)
            _ = new CoroutineTask(BatchLoop());
    }

    private class PendingSpawn { public Info info; }
    private class PendingUnload { public string uid; public int id; public Vector3 pos; }
    private static readonly List<PendingSpawn> _pendingSpawns = new List<PendingSpawn>();
    private static readonly List<PendingUnload> _pendingUnloads = new List<PendingUnload>();

    /// <summary>Client-side: tracks last received animation hash per uid so we know when to play a new one-shot.</summary>
    private static readonly Dictionary<string, int> _lastReceivedAnimHash = new Dictionary<string, int>();

    /// <summary>Host-side: container UIDs whose storage has already been sent to clients.
    /// Prevents redundant StorageSyncMessage on every batch tick.</summary>
    private static readonly HashSet<string> _containerStorageSent = new HashSet<string>();

    private static void OnBatchEntityInfoMessageReceived(BatchEntityInfoMessage message)
    {
        // Host generates entities locally — skip own broadcast to avoid duplicates
        if (Helper.IsHost()) return;
        int n = message.uids.Length;
        for (int i = 0; i < n; i++)
        {
            string uid = message.uids[i];
            if (string.IsNullOrEmpty(uid)) continue;
            int id = message.ids[i];
            Vector3 pos = message.positions[i];
            bool destroyed = message.destroyed[i];
            int ownerId = (message.ownerIds != null && i < message.ownerIds.Length) ? message.ownerIds[i] : 0;
            Vector3 animDir = message.animDirections[i];
            bool animGrounded = message.animIsGrounded[i];
            float animSpeedCurr = message.animSpeedCurrent[i];
            float animSpeedTarg = message.animSpeedTarget[i];
            bool animFace = message.animFaceTarget[i];
            Vector3 animTargetScreen = message.animTargetScreenDirs[i];
            int animTrigger = 0;
            float animNormalizedTime = 0f;
            if (message.animTriggers != null && i < message.animTriggers.Length)
            {
                animTrigger = message.animTriggers[i];
                if (message.animNormalizedTimes != null && i < message.animNormalizedTimes.Length)
                    animNormalizedTime = message.animNormalizedTimes[i];
            }
            bool isExist = InfoMap.TryGetValue(uid, out Info targetInfo);
            if (!isExist)
            {
                // create the correct typed Info without full spawn serialization
                Info info = Entity.CreateInfo((ID)id, pos);
                info.uid = uid;
                info.ownerId = ownerId;
                info.position = pos;
                // Ensure ItemInfo has a valid item slot (batch sends item ID but CreateInfo
                // may leave item null if the ID is in Entity.Dictionary).
                // Always apply the stack count from the batch — CreateInfo sets Stack=1 by default,
                // so without this all stacked items appear as single items on the client.
                if (info is ItemInfo itemInfo)
                {
                    if (itemInfo.item == null || itemInfo.item.ID == ID.Null)
                        itemInfo.item = new ItemSlot((ID)id);
                    if (message.itemAmounts != null && i < message.itemAmounts.Length && message.itemAmounts[i] > 1)
                        itemInfo.item.Stack = message.itemAmounts[i];
                    if (message.itemDurabilities != null && i < message.itemDurabilities.Length && message.itemDurabilities[i] > 0)
                        itemInfo.item.Durability = message.itemDurabilities[i];
                }
                InfoMap[uid] = info;
                Entity.SpawnFromInfo(info, true);
                isExist = true;
                targetInfo = info;
            }
            else
            {
                // Update ownerId even for existing entities (e.g. after ownership transfer)
                targetInfo.ownerId = ownerId;
            }
            if (destroyed)
            {
                if (targetInfo.Machine != null)
                    ((EntityMachine)targetInfo.Machine).Unload();
                InfoMap.Remove(uid);
                continue;
            }

            // Entity might have been destroyed locally (pickup via F/right-click in client-authoritative mode).
            // If Machine was nulled by Unload(), just skip — the server will broadcast the destroy soon.
            if (targetInfo.Machine == null)
                continue;

            // Skip sync for entities we own — we run them locally
            if (targetInfo.IsOwner())
                continue;

            // Non-player entities are host-owned — skip syncing any that slipped through
            // (e.g. stale state). Host broadcasts are authoritative for all non-player entities.
            if (targetInfo.ownerId != 0)
                continue;

            // Update minimal authoritative fields
            targetInfo.position = pos;
            EntityMachine target = (EntityMachine)targetInfo.Machine;
            target.transform.position = targetInfo.position;

            // Apply animation/runtime primitives
            if (targetInfo is DynamicInfo dyn)
            {
                dyn.Direction = animDir;
                dyn.IsGrounded = animGrounded;
                dyn.SpeedCurrent = animSpeedCurr;
                dyn.SpeedTarget = animSpeedTarg;
                dyn.TargetScreenDir = WorldAlignedToScreen(animTargetScreen);
            }
            if (targetInfo is MobInfo mob)
            {
                mob.FaceTarget = animFace;
            }

            // One-shot animation trigger: play if hash changed
            if (animTrigger != 0 && targetInfo is DynamicInfo dynAnim)
            {
                int prevHash = _lastReceivedAnimHash.GetValueOrDefault(uid, 0);
                if (animTrigger != prevHash && dynAnim.Animator != null && dynAnim.Animator.isActiveAndEnabled)
                {
                    dynAnim.Animator.Play(animTrigger, 0, animNormalizedTime);
                }
                _lastReceivedAnimHash[uid] = animTrigger;
            }
            else
            {
                _lastReceivedAnimHash.Remove(uid);
            }
        }
    }

    // Collect all currently loaded entities and send them in one batched message.
    // Reusable batch buffers to reduce per-tick allocations
    private static readonly List<string> _batchUids = new List<string>();
    private static readonly List<int> _batchIds = new List<int>();
    private static readonly List<Vector3> _batchPositions = new List<Vector3>();
    private static readonly List<bool> _batchDestroyed = new List<bool>();
    private static readonly List<Vector3> _batchAnimDirs = new List<Vector3>();
    private static readonly List<bool> _batchAnimGrounded = new List<bool>();
    private static readonly List<float> _batchAnimSpeedCurr = new List<float>();
    private static readonly List<float> _batchAnimSpeedTarg = new List<float>();
    private static readonly List<bool> _batchAnimFace = new List<bool>();
    private static readonly List<Vector3> _batchAnimTargetScreens = new List<Vector3>();
    private static readonly List<int> _batchAnimTriggers = new List<int>();
    private static readonly List<float> _batchAnimNormalizedTimes = new List<float>();
    private static readonly List<int> _batchItemAmounts = new List<int>();
    private static readonly List<int> _batchItemDurabilities = new List<int>();
    private static readonly List<int> _batchOwnerIds = new List<int>();

    public static void SendBatch()
    {
        if (!NetworkServer.active) return;
        _batchUids.Clear(); _batchIds.Clear(); _batchPositions.Clear(); _batchDestroyed.Clear();
        _batchAnimDirs.Clear(); _batchAnimGrounded.Clear(); _batchAnimSpeedCurr.Clear();
        _batchAnimSpeedTarg.Clear(); _batchAnimFace.Clear(); _batchAnimTargetScreens.Clear();
        _batchAnimTriggers.Clear(); _batchAnimNormalizedTimes.Clear();
        _batchItemAmounts.Clear(); _batchItemDurabilities.Clear(); _batchOwnerIds.Clear();
        void AddAnimData(DynamicInfo dyn)
        {
            _batchAnimDirs.Add(dyn.Direction);
            _batchAnimGrounded.Add(dyn.IsGrounded);
            _batchAnimSpeedCurr.Add(dyn.SpeedCurrent);
            _batchAnimSpeedTarg.Add(dyn.SpeedTarget);
            _batchAnimFace.Add((dyn is MobInfo mob) ? mob.FaceTarget : false);
            _batchAnimTargetScreens.Add(ScreenToWorldAligned(dyn.TargetScreenDir));
            if (dyn.Animator != null && dyn.Animator.isActiveAndEnabled)
            {
                AnimatorStateInfo state = dyn.Animator.GetCurrentAnimatorStateInfo(0);
                _batchAnimTriggers.Add(state.shortNameHash);
                _batchAnimNormalizedTimes.Add(state.normalizedTime);
            }
            else
            {
                _batchAnimTriggers.Add(0);
                _batchAnimNormalizedTimes.Add(0f);
            }
        }

        void AddZeroAnimData()
        {
            _batchAnimDirs.Add(Vector3.zero);
            _batchAnimGrounded.Add(false);
            _batchAnimSpeedCurr.Add(0f);
            _batchAnimSpeedTarg.Add(0f);
            _batchAnimFace.Add(false);
            _batchAnimTargetScreens.Add(Vector3.zero);
            _batchAnimTriggers.Add(0);
            _batchAnimNormalizedTimes.Add(0f);
        }

        void AddEntityToBatch(EntityMachine em)
        {
            if (em == null || em.Info == null) return;
            _batchUids.Add(em.Info.uid);
            _batchOwnerIds.Add(em.Info.ownerId);
            _batchIds.Add((int)(em.Info is ItemInfo { item: not null } ii ? ii.item.ID : em.Info.id));
            _batchPositions.Add(em.Info.position);
            _batchDestroyed.Add(em.Info.Destroyed);
            _batchItemAmounts.Add(em.Info is ItemInfo { item: not null } ia ? ia.item.Stack : 0);
            _batchItemDurabilities.Add(em.Info is ItemInfo { item: not null } ida ? ida.item.Durability : 0);

            if (em.Info is DynamicInfo dyn)
                AddAnimData(dyn);
            else
                AddZeroAnimData();
        }

        foreach (var em in EntityDynamicLoad.ActiveEntities)
            AddEntityToBatch(em);

        // Pending spawns: one-shot entities (e.g. newly placed structures) that
        // only need a single broadcast to appear on clients.
        foreach (var ps in _pendingSpawns)
        {
            if (ps.info?.Machine is EntityMachine em)
                AddEntityToBatch(em);
        }
        _pendingSpawns.Clear();

        // append pending unloads (processed BEFORE the empty check so that
        // the last entity's destroy is always broadcast to clients)
        foreach (var pu in _pendingUnloads)
        {
            _batchUids.Add(pu.uid);
            _batchOwnerIds.Add(0);
            _batchIds.Add(pu.id);
            _batchPositions.Add(pu.pos);
            _batchDestroyed.Add(true);
            _batchItemAmounts.Add(0);
            _batchItemDurabilities.Add(0);
            AddZeroAnimData();
        }
        _pendingUnloads.Clear();

        if (_batchUids.Count == 0) return;

        NetworkServer.SendToAll(new BatchEntityInfoMessage
        {
            uids = _batchUids.ToArray(),
            ids = _batchIds.ToArray(),
            positions = _batchPositions.ToArray(),
            destroyed = _batchDestroyed.ToArray(),
            ownerIds = _batchOwnerIds.ToArray(),
            animDirections = _batchAnimDirs.ToArray(),
            animIsGrounded = _batchAnimGrounded.ToArray(),
            animSpeedCurrent = _batchAnimSpeedCurr.ToArray(),
            animSpeedTarget = _batchAnimSpeedTarg.ToArray(),
            animFaceTarget = _batchAnimFace.ToArray(),
            animTargetScreenDirs = _batchAnimTargetScreens.ToArray(),
            animTriggers = _batchAnimTriggers.ToArray(),
            animNormalizedTimes = _batchAnimNormalizedTimes.ToArray(),
            itemAmounts = _batchItemAmounts.ToArray(),
            itemDurabilities = _batchItemDurabilities.ToArray()
        });

        // Send initial storage for NEW container entities (StorageSync only sends on modification).
        // The batch message creates the entity on the client; storage follows immediately
        // so Mirror's in-order delivery guarantees the entity exists when StorageSync is applied.
        void SendContainerStorageFor(EntityMachine em)
        {
            if (em == null || em.Info is not ContainerInfo container || container.Storage?.List == null) return;
            if (!_containerStorageSent.Add(container.uid)) return; // already sent once
            NetworkServer.SendToAll(new StorageSyncMessage
            {
                entityUid = container.uid,
                storageData = Helper.SerializeObject(container.Storage.List)
            });
        }
        foreach (var em in EntityDynamicLoad.ActiveEntities) SendContainerStorageFor(em);
        foreach (var kv in EntityStaticLoad.ActiveEntities)
            foreach (var em in kv.Value.Item2) SendContainerStorageFor(em);
    }

    /// <summary>Non-player entities are always host-owned (ownerId=0).
    /// Free players (controllerId=-1) also stay host-owned since ownership transfer
    /// to a remote client causes pathfinding bugs (lost target/action context,
    /// ownership thrashing at range edges, and other clients skipping position sync).</summary>

    private static IEnumerator BatchLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(BroadcastInterval);
            if (!NetworkServer.active) continue;
            SendBatch();
        }
    }

    /// <summary>Non-player entities are host-owned — clients never send entity updates.
    /// Client-side animation/runtime sync is handled entirely via the host's BatchEntityInfoMessage broadcast.</summary>

    public static void BroadcastEntityUnload(Info info)
    {
        if (!Helper.IsHost() || !NetworkServer.active || info == null) return;
        _pendingUnloads.Add(new PendingUnload { uid = info.uid, id = (int)info.id, pos = info.position });
    }

    /// <summary>Queue a one-time spawn broadcast for a newly created entity.
    /// Used by static-load entities (structures) that aren't in the dynamic batch loop.</summary>
    public static void BroadcastEntitySpawn(Info info)
    {
        if (!Helper.IsHost() || !NetworkServer.active || info == null || info.Machine == null) return;
        _pendingSpawns.Add(new PendingSpawn { info = info });
    }

    // ── Shared coordinate transforms ─────────────────────────────

    /// <summary>Convert screen-space direction to world-aligned (camera-independent).</summary>
    public static Vector3 ScreenToWorldAligned(Vector3 screenDir)
    {
        float orbitRad = ViewPort.OrbitRotation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(orbitRad);
        float sin = Mathf.Sin(orbitRad);
        return new Vector3(
            screenDir.x * cos + screenDir.y * sin,
            -screenDir.x * sin + screenDir.y * cos,
            0);
    }

    /// <summary>Convert world-aligned direction back to local screen-space.</summary>
    public static Vector3 WorldAlignedToScreen(Vector3 worldDir)
    {
        float orbitRad = ViewPort.OrbitRotation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(orbitRad);
        float sin = Mathf.Sin(orbitRad);
        return new Vector3(
            worldDir.x * cos - worldDir.y * sin,
            worldDir.x * sin + worldDir.y * cos,
            0);
    }

    public static void Clear()
    {
        _containerStorageSent.Clear();
    }

    // Single-entity handler removed; batching only.
}
