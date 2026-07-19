using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// Sent when a client first connects — transfers the entire Save object
/// (all worlds / dimensions).
/// </summary>
public struct HostToClientFullSaveMessage : NetworkMessage
{
    public int index;
    public int totalChunks;
    public byte[] data;
}

/// <summary>
/// Sent when the host switches to a different world (dimension) —
/// transfers a single World so the client can follow.
/// </summary>
public struct HostToClientWorldSwitchMessage : NetworkMessage
{
    public int index;
    public int totalChunks;
    public byte[] data;
    public GenType worldType;
}

public static class SaveSync
{
    // Shared reassembly state (only one transfer can be in-flight at a time).
    private static readonly Dictionary<int, byte[]> _receivedChunks = new();
    private static int _expectedChunks;
    private static bool _isReceivingWorldSwitch;
    private static GenType _pendingWorldType;

    public static void RegisterHandlers()
    {
        NetworkClient.ReplaceHandler<HostToClientFullSaveMessage>(OnFullSaveMessage, false);
        NetworkClient.ReplaceHandler<HostToClientWorldSwitchMessage>(OnWorldSwitchMessage, false);
    }

    /// <summary>
    /// Sends the full Save object (all worlds) to a newly connected client.
    /// </summary>
    public static void SendFullSave(NetworkConnectionToClient conn, Save save)
    {
        if (!NetworkServer.active || conn == null || save == null) return;

        byte[] bytes = Helper.SerializeObject(save);
        byte[][] chunks = Helper.SplitBytes(bytes, Server.MaxSnapshotChunkSize);
        int totalChunks = chunks.Length;

        for (int i = 0; i < totalChunks; i++)
        {
            conn.Send(new HostToClientFullSaveMessage
            {
                index = i,
                totalChunks = totalChunks,
                data = chunks[i]
            });
        }
    }

    /// <summary>
    /// Sends a single world (dimension) to a specific client — used when
    /// the host switches worlds so the client can download the new world data.
    /// </summary>
    public static void SendWorldSwitch(NetworkConnectionToClient conn, GenType worldType)
    {
        if (!NetworkServer.active || conn == null) return;

        World world = Save.Inst.worlds[worldType];
        if (world == null) return;

        byte[] bytes = Helper.SerializeObject(world);
        byte[][] chunks = Helper.SplitBytes(bytes, Server.MaxSnapshotChunkSize);
        int totalChunks = chunks.Length;

        for (int i = 0; i < totalChunks; i++)
        {
            conn.Send(new HostToClientWorldSwitchMessage
            {
                index = i,
                totalChunks = totalChunks,
                data = chunks[i],
                worldType = worldType
            });
        }
    }

    /// <summary>
    /// Broadcasts a world (dimension) switch to all connected clients.
    /// </summary>
    public static void BroadcastWorldSwitch(GenType worldType)
    {
        if (!NetworkServer.active) return;

        World world = Save.Inst.worlds[worldType];
        if (world == null) return;

        byte[] bytes = Helper.SerializeObject(world);
        byte[][] chunks = Helper.SplitBytes(bytes, Server.MaxSnapshotChunkSize);
        int total = chunks.Length;

        foreach (var kv in NetworkServer.connections)
        {
            if (!kv.Value.isReady) continue;
            for (int i = 0; i < total; i++)
            {
                kv.Value.Send(new HostToClientWorldSwitchMessage
                {
                    index = i,
                    totalChunks = total,
                    data = chunks[i],
                    worldType = worldType
                });
            }
        }
    }

    // ── Receive handlers ──────────────────────────────────────────────

    private static void OnFullSaveMessage(HostToClientFullSaveMessage message)
    {
        if (!BeginReceive(message.totalChunks)) return;
        _isReceivingWorldSwitch = false;

        _receivedChunks[message.index] = message.data;
        if (_receivedChunks.Count < _expectedChunks) return;

        byte[] allBytes = FinishReceive();
        Save save = Helper.DeserializeObject<Save>(allBytes);
        if (save == null) return;

        save.id = null;
        Save.Inst = save;
        Scene.SwitchSave(save);
    }

    private static void OnWorldSwitchMessage(HostToClientWorldSwitchMessage message)
    {
        if (!BeginReceive(message.totalChunks, message.worldType)) return;
        _isReceivingWorldSwitch = true;

        _receivedChunks[message.index] = message.data;
        if (_receivedChunks.Count < _expectedChunks) return;

        byte[] allBytes = FinishReceive();
        World world = Helper.DeserializeObject<World>(allBytes);
        if (world == null) return;

        Save.Inst.worlds[_pendingWorldType] = world;
    }

    /// <summary>
    /// Returns false if the chunk is a duplicate (already received).
    /// Resets state if the sequence changed mid-stream.
    /// </summary>
    private static bool BeginReceive(int totalChunks, GenType worldType = default)
    {
        if (_expectedChunks == 0)
        {
            _expectedChunks = totalChunks;
            _pendingWorldType = worldType;
            return true;
        }

        // Sequence mismatch — reset and start over
        if (totalChunks != _expectedChunks || (_isReceivingWorldSwitch && worldType != _pendingWorldType))
        {
            _expectedChunks = totalChunks;
            _pendingWorldType = worldType;
            _receivedChunks.Clear();
        }

        return true;
    }

    private static byte[] FinishReceive()
    {
        byte[] allBytes = Helper.CombineChunks(_receivedChunks, _expectedChunks);
        _receivedChunks.Clear();
        _expectedChunks = 0;
        _isReceivingWorldSwitch = false;
        return allBytes;
    }

    public static void Clear()
    {
        _receivedChunks.Clear();
        _expectedChunks = 0;
        _isReceivingWorldSwitch = false;
    }
}
