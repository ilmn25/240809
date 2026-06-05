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
                ((EntityMachine)targetInfo.Machine).Unload();
                InfoMap.Remove(uid);
                continue;
            }

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
                dyn.TargetScreenDir = animTargetScreen;
            }
            if (targetInfo is MobInfo mob)
            {
                mob.FaceTarget = animFace;
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
        // no full Info bytes list anymore

        List<Vector3> animDirs = new List<Vector3>();
        List<bool> animGrounded = new List<bool>();
        List<float> animSpeedCurr = new List<float>();
        List<float> animSpeedTarg = new List<float>();
        List<bool> animFace = new List<bool>();
        List<Vector3> animTargetScreens = new List<Vector3>();

        foreach (var em in EntityDynamicLoad.GetActiveEntities())
        {
            if (em == null) continue;
            uids.Add(em.Info.uid);
            ids.Add((int)em.Info.id);
            positions.Add(em.Info.position);
            destroyed.Add(em.Info.Destroyed);

                // placeholder for alignment
                // (no spawn bytes sent)

            if (em.Info is DynamicInfo dyn)
            {
                animDirs.Add(dyn.Direction);
                animGrounded.Add(dyn.IsGrounded);
                animSpeedCurr.Add(dyn.SpeedCurrent);
                animSpeedTarg.Add(dyn.SpeedTarget);
                animFace.Add((em.Info is MobInfo mob) ? mob.FaceTarget : false);
                animTargetScreens.Add(dyn.TargetScreenDir);
            }
            else
            {
                animDirs.Add(Vector3.zero);
                animGrounded.Add(false);
                animSpeedCurr.Add(0f);
                animSpeedTarg.Add(0f);
                animFace.Add(false);
                animTargetScreens.Add(Vector3.zero);
            }
        }

        foreach (var kv in EntityStaticLoad.ActiveEntities)
        {
            foreach (var em in kv.Value.Item2)
            {
                if (em == null) continue;
                uids.Add(em.Info.uid);
                ids.Add((int)em.Info.id);
                positions.Add(em.Info.position);
                destroyed.Add(em.Info.Destroyed);

                // no spawn bytes; clients will instantiate from minimal id/position
                // add placeholder null to keep arrays aligned
                // (we don't include spawn bytes)

                if (em.Info is DynamicInfo dyn)
                {
                    animDirs.Add(dyn.Direction);
                    animGrounded.Add(dyn.IsGrounded);
                    animSpeedCurr.Add(dyn.SpeedCurrent);
                    animSpeedTarg.Add(dyn.SpeedTarget);
                    animFace.Add((em.Info is MobInfo mob) ? mob.FaceTarget : false);
                    animTargetScreens.Add(dyn.TargetScreenDir);
                }
                else
                {
                    animDirs.Add(Vector3.zero);
                    animGrounded.Add(false);
                    animSpeedCurr.Add(0f);
                    animSpeedTarg.Add(0f);
                    animFace.Add(false);
                    animTargetScreens.Add(Vector3.zero);
                }
            }
        }

        if (uids.Count == 0) return;

        // append pending unloads
        foreach (var pu in _pendingUnloads)
        {
            uids.Add(pu.uid);
            ids.Add(pu.id);
            positions.Add(pu.pos);
            destroyed.Add(true);
            // no spawn bytes
            animDirs.Add(Vector3.zero);
            animGrounded.Add(false);
            animSpeedCurr.Add(0f);
            animSpeedTarg.Add(0f);
            animFace.Add(false);
            animTargetScreens.Add(Vector3.zero);
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
            animTargetScreenDirs = animTargetScreens.ToArray()
        });
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

    // Single-entity handler removed; batching only.
}
