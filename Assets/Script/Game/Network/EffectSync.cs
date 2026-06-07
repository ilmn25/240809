using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public struct BatchParticleEventMessage : NetworkMessage
{
    public int[] effectIds;      // (int)Particles enum
    public Vector3[] positions;
    public bool[] forces;
}

public static class EffectSync
{
    public static float BroadcastInterval = 0.05f; // 50ms

    private struct PendingParticle
    {
        public int effectId;
        public Vector3 position;
        public bool force;
    }

    private static readonly List<PendingParticle> _pendingParticles = new List<PendingParticle>();

    public static void RegisterHandlers()
    {
        NetworkClient.ReplaceHandler<BatchParticleEventMessage>(OnBatchParticleEventMessageReceived, false);

        if (Application.isPlaying)
        {
            _ = new CoroutineTask(BatchLoop());
        }
    }

    /// <summary>Called by Particle.Create on the host to queue a particle for broadcast.</summary>
    public static void EnqueueParticle(Vector3 position, Particles id, bool force)
    {
        if (!NetworkServer.active) return;
        _pendingParticles.Add(new PendingParticle
        {
            effectId = (int)id,
            position = position,
            force = force
        });
    }

    private static void SendBatch()
    {
        if (!NetworkServer.active || _pendingParticles.Count == 0) return;

        int n = _pendingParticles.Count;
        int[] ids = new int[n];
        Vector3[] positions = new Vector3[n];
        bool[] forces = new bool[n];
        for (int i = 0; i < n; i++)
        {
            ids[i] = _pendingParticles[i].effectId;
            positions[i] = _pendingParticles[i].position;
            forces[i] = _pendingParticles[i].force;
        }
        _pendingParticles.Clear();

        NetworkServer.SendToAll(new BatchParticleEventMessage
        {
            effectIds = ids,
            positions = positions,
            forces = forces
        });
    }

    private static void OnBatchParticleEventMessageReceived(BatchParticleEventMessage msg)
    {
        if (Helper.IsHost()) return;
        int n = msg.effectIds.Length;
        for (int i = 0; i < n; i++)
        {
            Particles id = (Particles)msg.effectIds[i];
            Vector3 pos = msg.positions[i];
            bool force = msg.forces[i];
            Particle.Create(pos, id, force);
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
