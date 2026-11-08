using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>Batch of particles broadcast from server to all clients.</summary>
public struct BatchParticleEventMessage : NetworkMessage
{
    public int[] effectIds;          // (int)Particles enum
    public Vector3[] positions;
    public bool[] forces;
    /// <summary>Entity uid each particle is anchored to (null/empty = world-space).</summary>
    public string[] followUids;
}

public static class EffectSync
{
    public static float BroadcastInterval = 0.05f; // 50ms

    private struct PendingParticle
    {
        public int effectId;
        public Vector3 position;
        public bool force;
        public string followUid;
    }

    private static readonly List<PendingParticle> _pendingParticles = new List<PendingParticle>();

    public static void RegisterHandlers()
    {
        // Server → Client broadcast
        NetworkClient.ReplaceHandler<BatchParticleEventMessage>(OnBatchParticleEventMessageReceived, false);

        if (Application.isPlaying)
        {
            _ = new CoroutineTask(BatchLoop());
        }
    }

    /// <summary>Called by Particle.Create to queue a particle for network sync.
    /// Only the host enqueues particles for broadcast; remote clients never relay.</summary>
    public static void EnqueueParticle(Vector3 position, Particles id, bool force, string followUid = null)
    {
        if (NetworkServer.active)
        {
            _pendingParticles.Add(new PendingParticle
            {
                effectId = (int)id,
                position = position,
                force = force,
                followUid = followUid
            });
        }
    }

    private static void SendBatch()
    {
        if (!NetworkServer.active || _pendingParticles.Count == 0) return;

        int n = _pendingParticles.Count;
        int[] ids = new int[n];
        Vector3[] positions = new Vector3[n];
        bool[] forces = new bool[n];
        string[] followUids = new string[n];
        for (int i = 0; i < n; i++)
        {
            ids[i] = _pendingParticles[i].effectId;
            positions[i] = _pendingParticles[i].position;
            forces[i] = _pendingParticles[i].force;
            followUids[i] = _pendingParticles[i].followUid;
        }
        _pendingParticles.Clear();

        var msg = new BatchParticleEventMessage
        {
            effectIds = ids,
            positions = positions,
            forces = forces,
            followUids = followUids
        };

        NetworkServer.SendToAll(msg);
    }

    private static void OnBatchParticleEventMessageReceived(BatchParticleEventMessage msg)
    {
        // Host mode loops broadcasts back into the local client, and the host already
        // spawned these particles when it enqueued them — spawning again doubles each one.
        if (Helper.IsHost()) return;

        int n = msg.effectIds.Length;
        for (int i = 0; i < n; i++)
        {
            Particles id = (Particles)msg.effectIds[i];
            Vector3 pos = msg.positions[i];
            bool force = msg.forces[i];
            string followUid = (msg.followUids != null && i < msg.followUids.Length) ? msg.followUids[i] : null;

            if (string.IsNullOrEmpty(followUid))
                Particle.Create(pos, id, force, sync: false);
            else
                Particle.CreateSynced(pos, id, force, followUid);
        }
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

    public static void Clear()
    {
        _pendingParticles.Clear();
    }
}
