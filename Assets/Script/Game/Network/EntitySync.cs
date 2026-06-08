using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Mirror;
using UnityEngine;

// Single-entity message removed; batching only.

public struct BatchEntityInfoMessage : NetworkMessage
{
    public string[] uids;
    public int[] ids;
    public Vector3[] positions;
    public bool[] destroyed;

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
}

public static class EntitySync
{
    public static readonly Dictionary<string, Info> InfoMap = new Dictionary<string, Info>();
    // batch send interval
    public static float BroadcastInterval = 0.025f;

    [System.Serializable]
    private struct AnimationState
    {
        public Vector3 Direction;
        public bool IsGrounded;
        public float SpeedCurrent;
        public float SpeedTarget;
        public bool FaceTarget;
        public Vector3 TargetScreenDir;
    }

    public static void RegisterHandlers()
    {
        NetworkClient.ReplaceHandler<BatchEntityInfoMessage>(OnBatchEntityInfoMessageReceived, false);

        // Start batch loop using CoroutineTask when running in play mode.
        if (Application.isPlaying)
        {
            _ = new CoroutineTask(BatchLoop());
        }
    }

    private class PendingUnload { public string uid; public int id; public Vector3 pos; }
    private static readonly List<PendingUnload> _pendingUnloads = new List<PendingUnload>();

    /// <summary>Client-side: tracks last received animation hash per uid so we know when to play a new one-shot.</summary>
    private static readonly Dictionary<string, int> _lastReceivedAnimHash = new Dictionary<string, int>();

    /// <summary>Host-side: container UIDs whose storage has already been sent to clients.
    /// Prevents redundant StorageSyncMessage on every batch tick.</summary>
    private static readonly HashSet<string> _containerStorageSent = new HashSet<string>();

    private static void OnBatchEntityInfoMessageReceived(BatchEntityInfoMessage message)
    {
        if (Helper.IsHost()) return;
        int n = message.uids.Length;
        for (int i = 0; i < n; i++)
        {
            string uid = message.uids[i];
            if (string.IsNullOrEmpty(uid)) continue;
            int id = message.ids[i];
            Vector3 pos = message.positions[i];
            bool destroyed = message.destroyed[i];
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
                info.position = pos;
                InfoMap[uid] = info;
                Entity.SpawnFromInfo(info, true);
                isExist = true;
                targetInfo = info;
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
                // Convert TargetScreenDir from world-aligned back to local screen-space
                float orbitRad = ViewPort.OrbitRotation * Mathf.Deg2Rad;
                float cos = Mathf.Cos(orbitRad);
                float sin = Mathf.Sin(orbitRad);
                dyn.TargetScreenDir = new Vector3(
                    animTargetScreen.x * cos - animTargetScreen.y * sin,
                    animTargetScreen.x * sin + animTargetScreen.y * cos,
                    0);
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
        void AddAnimData(DynamicInfo dyn)
        {
            animDirs.Add(dyn.Direction);
            animGrounded.Add(dyn.IsGrounded);
            animSpeedCurr.Add(dyn.SpeedCurrent);
            animSpeedTarg.Add(dyn.SpeedTarget);
            animFace.Add((dyn is MobInfo mob) ? mob.FaceTarget : false);
            // Convert screen-space TargetScreenDir to world-aligned before syncing
            Vector3 localDir = dyn.TargetScreenDir;
            float orbitRad = ViewPort.OrbitRotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(-orbitRad);
            float sin = Mathf.Sin(-orbitRad);
            animTargetScreens.Add(new Vector3(
                localDir.x * cos - localDir.y * sin,
                localDir.x * sin + localDir.y * cos,
                0));
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

        foreach (var em in EntityDynamicLoad.GetActiveEntities())
        {
            if (em == null) continue;
            uids.Add(em.Info.uid);
            // For items, send the actual item ID (e.g. ID.Log) instead of the generic ID.ItemPrefab
            // so the client can reconstruct the ItemInfo with a proper ItemSlot.
            ids.Add((int)(em.Info is ItemInfo { item: not null } ii ? ii.item.ID : em.Info.id));
            positions.Add(em.Info.position);
            destroyed.Add(em.Info.Destroyed);

            if (em.Info is DynamicInfo dyn)
                AddAnimData(dyn);
            else
                AddZeroAnimData();

        }

        if (uids.Count == 0) return;

        // append pending unloads
        foreach (var pu in _pendingUnloads)
        {
            uids.Add(pu.uid);
            ids.Add(pu.id);
            positions.Add(pu.pos);
            destroyed.Add(true);
            AddZeroAnimData();
        }
        _pendingUnloads.Clear();

        NetworkServer.SendToAll(new BatchEntityInfoMessage
        {
            uids = uids.ToArray(),
            ids = ids.ToArray(),
            positions = positions.ToArray(),
            destroyed = destroyed.ToArray(),
            animDirections = animDirs.ToArray(),
            animIsGrounded = animGrounded.ToArray(),
            animSpeedCurrent = animSpeedCurr.ToArray(),
            animSpeedTarget = animSpeedTarg.ToArray(),
            animFaceTarget = animFace.ToArray(),
            animTargetScreenDirs = animTargetScreens.ToArray(),
            animTriggers = animTriggers.ToArray(),
            animNormalizedTimes = animNormalizedTimes.ToArray()
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
        }
    }

    public static void BroadcastEntityUnload(Info info)
    {
        if (!Helper.IsHost() || !NetworkServer.active || info == null) return;
        _pendingUnloads.Add(new PendingUnload { uid = info.uid, id = (int)info.id, pos = info.position });
    }

    public static void Clear()
    {
        _containerStorageSent.Clear();
    }

    // Single-entity handler removed; batching only.
}
