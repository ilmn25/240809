using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>Batch of particles broadcast from server to all clients.
/// sourceConnectionIds[i] = 0 means host-originated; otherwise the originating client's connectionId.
/// Receiving clients skip particles whose sourceConnectionId matches their own connectionId.</summary>
public struct BatchParticleEventMessage : NetworkMessage
{
    public int[] effectIds;          // (int)Particles enum
    public Vector3[] positions;
    public bool[] forces;
    public int[] sourceConnectionIds; // 0 = host, else remote client's connectionId
}

/// <summary>Single particle sent from a remote client to the server for relay.</summary>
public struct ClientParticleMessage : NetworkMessage
{
    public int effectId;
    public Vector3 position;
    public bool force;
}

public static class EffectSync
{
    public static float BroadcastInterval = 0.05f; // 50ms

    private struct PendingParticle
    {
        public int effectId;
        public Vector3 position;
        public bool force;
        public int sourceConnectionId; // 0 = host, else remote client's connectionId
    }

    private static readonly List<PendingParticle> _pendingParticles = new List<PendingParticle>();

    public static void RegisterHandlers()
    {
        // Client → Server (remote client relay)
        NetworkServer.ReplaceHandler<ClientParticleMessage>(OnServerClientParticle, false);

        // Server → Client broadcast
        NetworkClient.ReplaceHandler<BatchParticleEventMessage>(OnBatchParticleEventMessageReceived, false);

        if (Application.isPlaying)
        {
            _ = new CoroutineTask(BatchLoop());
        }
    }

    /// <summary>Called by Particle.Create to queue a particle for network sync.
    /// On the server/host, adds to the batch loop for periodic broadcast.
    /// On a remote client, immediately sends to the server for relay.</summary>
    public static void EnqueueParticle(Vector3 position, Particles id, bool force)
    {
        if (NetworkServer.active)
        {
            _pendingParticles.Add(new PendingParticle
            {
                effectId = (int)id,
                position = position,
                force = force,
                sourceConnectionId = 0 // host-originated
            });
        }
        else if (NetworkClient.isConnected)
        {
            NetworkClient.Send(new ClientParticleMessage
            {
                effectId = (int)id,
                position = position,
                force = force
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
        int[] sourceIds = new int[n];
        for (int i = 0; i < n; i++)
        {
            ids[i] = _pendingParticles[i].effectId;
            positions[i] = _pendingParticles[i].position;
            forces[i] = _pendingParticles[i].force;
            sourceIds[i] = _pendingParticles[i].sourceConnectionId;
        }
        _pendingParticles.Clear();

        var msg = new BatchParticleEventMessage
        {
            effectIds = ids,
            positions = positions,
            forces = forces,
            sourceConnectionIds = sourceIds
        };

        NetworkServer.SendToAll(msg);
    }

    private static void OnServerClientParticle(NetworkConnectionToClient conn, ClientParticleMessage msg)
    {
        // Remote client sent a particle — enqueue it for batched broadcast to all clients.
        _pendingParticles.Add(new PendingParticle
        {
            effectId = msg.effectId,
            position = msg.position,
            force = msg.force,
            sourceConnectionId = conn.connectionId
        });
    }

    private static void OnBatchParticleEventMessageReceived(BatchParticleEventMessage msg)
    {
        // Host client's connectionId is always 0; remote clients get theirs via YourConnectionIdMessage.
        int myConnectionId = Helper.IsHost() ? 0 : PlayerSync.MyConnectionId;
        int n = msg.effectIds.Length;
        for (int i = 0; i < n; i++)
        {
            // Skip particles that originated from this client (already instantiated locally)
            if (msg.sourceConnectionIds[i] == myConnectionId)
                continue;

            Particles id = (Particles)msg.effectIds[i];
            Vector3 pos = msg.positions[i];
            bool force = msg.forces[i];
            Particle.Create(pos, id, force, sync: false);
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
