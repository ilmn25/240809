using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

// Single-entity message removed; batching only.

public struct BatchEntityInfoMessage : NetworkMessage
{
    public string[] uids;
    public int[] ids;
    public Vector3[] positions;
    public bool[] destroyed;
    /// <summary>Owner connection ID for each entity. Empty = host-owned.</summary>
    public string[] ownerIds;

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

/// <summary>Owner client → Host: updated state for entities this client owns.
/// Host relays to all other clients.</summary>
public struct ClientEntityUpdateMessage : NetworkMessage
{
    public string[] uids;
    public Vector3[] positions;
    public Vector3[] animDirections;
    public bool[] animIsGrounded;
    public float[] animSpeedCurrent;
    public float[] animSpeedTarget;
    public int[] animTriggers;
    public float[] animNormalizedTimes;
}

public static class EntitySync
{
    public static readonly Dictionary<string, Info> InfoMap = new Dictionary<string, Info>();
    // batch send interval
    public static float BroadcastInterval = 0.025f;

    public static void RegisterHandlers()
    {
        NetworkClient.ReplaceHandler<BatchEntityInfoMessage>(OnBatchEntityInfoMessageReceived, false);
        NetworkClient.ReplaceHandler<ClientEntityUpdateMessage>(OnClientEntityUpdateReceived, false);
        // Host relays owner-client updates to other clients
        NetworkServer.ReplaceHandler<ClientEntityUpdateMessage>(OnRelayClientEntityUpdate, false);

        // Start batch loop using CoroutineTask when running in play mode.
        if (Application.isPlaying)
        {
            _ = new CoroutineTask(BatchLoop());
        }
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
            string ownerId = (message.ownerIds != null && i < message.ownerIds.Length) ? message.ownerIds[i] : "0";
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

            // Skip sync for client-owned entities — owner sends updates via relay
            if (targetInfo.ownerId != "0" && targetInfo.ownerId != "-1")
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
    public static void SendBatch()
    {
        if (!NetworkServer.active) return;
        List<string> uids = new List<string>();
        List<int> ids = new List<int>();
        List<Vector3> positions = new List<Vector3>();
        List<bool> destroyed = new List<bool>();

        List<Vector3> animDirs = new List<Vector3>();
        List<bool> animGrounded = new List<bool>();
        List<float> animSpeedCurr = new List<float>();
        List<float> animSpeedTarg = new List<float>();
        List<bool> animFace = new List<bool>();
        List<Vector3> animTargetScreens = new List<Vector3>();
        List<int> animTriggers = new List<int>();
        List<float> animNormalizedTimes = new List<float>();
        List<int> itemAmounts = new List<int>();
        List<int> itemDurabilities = new List<int>();
        List<string> ownerIds = new List<string>();
        void AddAnimData(DynamicInfo dyn)
        {
            animDirs.Add(dyn.Direction);
            animGrounded.Add(dyn.IsGrounded);
            animSpeedCurr.Add(dyn.SpeedCurrent);
            animSpeedTarg.Add(dyn.SpeedTarget);
            animFace.Add((dyn is MobInfo mob) ? mob.FaceTarget : false);
            animTargetScreens.Add(ScreenToWorldAligned(dyn.TargetScreenDir));
            // Read current Animator state for one-shot detection
            if (dyn.Animator != null && dyn.Animator.isActiveAndEnabled)
            {
                AnimatorStateInfo state = dyn.Animator.GetCurrentAnimatorStateInfo(0);
                animTriggers.Add(state.shortNameHash);
                animNormalizedTimes.Add(state.normalizedTime);
            }
            else
            {
                animTriggers.Add(0);
                animNormalizedTimes.Add(0f);
            }

        }

        void AddZeroAnimData()
        {
            animDirs.Add(Vector3.zero);
            animGrounded.Add(false);
            animSpeedCurr.Add(0f);
            animSpeedTarg.Add(0f);
            animFace.Add(false);
            animTargetScreens.Add(Vector3.zero);
            animTriggers.Add(0);
            animNormalizedTimes.Add(0f);
        }

        void AddEntityToBatch(EntityMachine em)
        {
            if (em == null || em.Info == null) return;
            uids.Add(em.Info.uid);
            ownerIds.Add(em.Info.ownerId);
            // For items, send the actual item ID (e.g. ID.Log) instead of the generic ID.ItemPrefab
            // so the client can reconstruct the ItemInfo with a proper ItemSlot.
            ids.Add((int)(em.Info is ItemInfo { item: not null } ii ? ii.item.ID : em.Info.id));
            positions.Add(em.Info.position);
            destroyed.Add(em.Info.Destroyed);
            itemAmounts.Add(em.Info is ItemInfo { item: not null } ia ? ia.item.Stack : 0);
            itemDurabilities.Add(em.Info is ItemInfo { item: not null } ida ? ida.item.Durability : 0);

            if (em.Info is DynamicInfo dyn)
                AddAnimData(dyn);
            else
                AddZeroAnimData();
        }

        foreach (var em in EntityDynamicLoad.GetActiveEntities())
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
            uids.Add(pu.uid);
            ownerIds.Add(""); // host-owned; unload doesn't need owner
            ids.Add(pu.id);
            positions.Add(pu.pos);
            destroyed.Add(true);
            itemAmounts.Add(0);
            itemDurabilities.Add(0);
            AddZeroAnimData();
        }
        _pendingUnloads.Clear();

        if (uids.Count == 0) return;

        NetworkServer.SendToAll(new BatchEntityInfoMessage
        {
            uids = uids.ToArray(),
            ids = ids.ToArray(),
            positions = positions.ToArray(),
            destroyed = destroyed.ToArray(),
            ownerIds = ownerIds.ToArray(),
            animDirections = animDirs.ToArray(),
            animIsGrounded = animGrounded.ToArray(),
            animSpeedCurrent = animSpeedCurr.ToArray(),
            animSpeedTarget = animSpeedTarg.ToArray(),
            animFaceTarget = animFace.ToArray(),
            animTargetScreenDirs = animTargetScreens.ToArray(),
            animTriggers = animTriggers.ToArray(),
            animNormalizedTimes = animNormalizedTimes.ToArray(),
            itemAmounts = itemAmounts.ToArray(),
            itemDurabilities = itemDurabilities.ToArray()
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
        foreach (var em in EntityDynamicLoad.GetActiveEntities()) SendContainerStorageFor(em);
        foreach (var kv in EntityStaticLoad.ActiveEntities)
            foreach (var em in kv.Value.Item2) SendContainerStorageFor(em);
    }

    private static IEnumerator BatchLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(BroadcastInterval);
            if (NetworkServer.active)
                SendBatch();
            else
                ClientSendUpdate();
        }
    }

    private static void ApplyEntityUpdate(string uid, Vector3 pos, Vector3 dir, bool grounded, float speedCurr, float speedTarg)
    {
        if (!InfoMap.TryGetValue(uid, out Info info))
            Info.Dictionary.TryGetValue(uid, out info);
        if (info == null || info.Machine == null || info.IsOwner()) return;
        info.position = pos;
        info.Machine.transform.position = pos;
        if (info is DynamicInfo dyn)
        {
            dyn.Direction = dir;
            dyn.IsGrounded = grounded;
            dyn.SpeedCurrent = speedCurr;
            dyn.SpeedTarget = speedTarg;
        }
    }

    /// <summary>Remote client: apply owner's entity updates relayed by host.</summary>
    private static void OnClientEntityUpdateReceived(ClientEntityUpdateMessage msg)
    {
        for (int i = 0; i < msg.uids.Length; i++)
            ApplyEntityUpdate(msg.uids[i], msg.positions[i],
                msg.animDirections[i], msg.animIsGrounded[i],
                msg.animSpeedCurrent[i], msg.animSpeedTarget[i]);
    }

    /// <summary>Host: apply owner update locally, then relay to remote clients.</summary>
    private static void OnRelayClientEntityUpdate(NetworkConnection conn, ClientEntityUpdateMessage msg)
    {
        if (!NetworkServer.active) return;
        int senderId = ((Mirror.NetworkConnectionToClient)conn).connectionId;
        for (int i = 0; i < msg.uids.Length; i++)
            ApplyEntityUpdate(msg.uids[i], msg.positions[i],
                msg.animDirections[i], msg.animIsGrounded[i],
                msg.animSpeedCurrent[i], msg.animSpeedTarget[i]);
        foreach (var kv in NetworkServer.connections)
            if (kv.Key != senderId && kv.Key != 0)
                kv.Value.Send(msg);
    }

    /// <summary>Client: send updated state for entities this client owns back to the host.</summary>
    private static void ClientSendUpdate()
    {
        if (Helper.IsHost() || !NetworkClient.active) return;
        List<string> uids = new List<string>();
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> animDirs = new List<Vector3>();
        List<bool> animGrounded = new List<bool>();
        List<float> animSpeedCurr = new List<float>();
        List<float> animSpeedTarg = new List<float>();
        List<int> animTriggers = new List<int>();
        List<float> animNormalizedTimes = new List<float>();

        foreach (var kv in InfoMap)
        {
            Info info = kv.Value;
            if (info == null || !info.IsOwner() || info.Machine == null) continue;
            uids.Add(info.uid);
            positions.Add(info.position);
            if (info is DynamicInfo dyn)
            {
                animDirs.Add(dyn.Direction);
                animGrounded.Add(dyn.IsGrounded);
                animSpeedCurr.Add(dyn.SpeedCurrent);
                animSpeedTarg.Add(dyn.SpeedTarget);
                if (dyn.Animator != null && dyn.Animator.isActiveAndEnabled)
                {
                    AnimatorStateInfo state = dyn.Animator.GetCurrentAnimatorStateInfo(0);
                    animTriggers.Add(state.shortNameHash);
                    animNormalizedTimes.Add(state.normalizedTime);
                }
                else
                {
                    animTriggers.Add(0);
                    animNormalizedTimes.Add(0f);
                }
            }
            else
            {
                animDirs.Add(Vector3.zero);
                animGrounded.Add(false);
                animSpeedCurr.Add(0f);
                animSpeedTarg.Add(0f);
                animTriggers.Add(0);
                animNormalizedTimes.Add(0f);
            }
        }
        if (uids.Count == 0) return;

        NetworkClient.Send(new ClientEntityUpdateMessage
        {
            uids = uids.ToArray(),
            positions = positions.ToArray(),
            animDirections = animDirs.ToArray(),
            animIsGrounded = animGrounded.ToArray(),
            animSpeedCurrent = animSpeedCurr.ToArray(),
            animSpeedTarget = animSpeedTarg.ToArray(),
            animTriggers = animTriggers.ToArray(),
            animNormalizedTimes = animNormalizedTimes.ToArray()
        });
    }

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
