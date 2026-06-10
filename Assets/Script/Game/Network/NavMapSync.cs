using Mirror;
using UnityEngine;

/// <summary>
/// Host serialises populated NavMap chunks and sends them to clients
/// so that client-side code (e.g. SpotLightModule) can read NavMap accurately.
/// </summary>
public struct NavMapSyncMessage : NetworkMessage
{
    public Vector3Int chunkCoord;
    public byte[] data; // packed NavMap bits for this chunk
}

/// <summary>Client → host: request all loaded NavMap data (sent after scene init).</summary>
public struct ClientNavMapRequestMessage : NetworkMessage { }

/// <summary>Incremental update for a single block change (~17 bytes vs 512 for full chunk).</summary>
public struct NavMapBlockUpdateMessage : NetworkMessage
{
    public Vector3Int blockCoord;
    public bool isAir;
}

public static class NavMapSync
{
    public static void RegisterHandlers()
    {
        NetworkClient.ReplaceHandler<NavMapSyncMessage>(OnNavMapSyncMessage, false);
        NetworkClient.ReplaceHandler<NavMapBlockUpdateMessage>(OnNavMapBlockUpdate, false);
        NetworkServer.ReplaceHandler<ClientNavMapRequestMessage>(OnClientNavMapRequest, false);
    }

    /// <summary>Host: broadcast the NavMap data for a single chunk to all clients.</summary>
    public static void BroadcastChunk(Vector3Int chunkCoord)
    {
        if (!NetworkServer.active) return;
        byte[] data = NavMap.PackChunk(chunkCoord);
        NetworkServer.SendToAll(new NavMapSyncMessage
        {
            chunkCoord = chunkCoord,
            data = data
        });
    }

    /// <summary>Host: broadcast an incremental single-block NavMap update to all clients.</summary>
    public static void BroadcastBlockUpdate(Vector3Int blockCoord, bool isAir)
    {
        if (!NetworkServer.active) return;
        NetworkServer.SendToAll(new NavMapBlockUpdateMessage
        {
            blockCoord = blockCoord,
            isAir = isAir
        });
    }

    /// <summary>Send NavMap data for all loaded chunks to a specific client (initial sync).</summary>
    public static void SendNavMapToClient(NetworkConnectionToClient conn)
    {
        if (!NetworkServer.active || conn == null) return;
        var loaded = NavMap.GetLoadedChunks();
        for (int i = 0; i < loaded.Count; i++)
        {
            byte[] data = NavMap.PackChunk(loaded[i]);
            conn.Send(new NavMapSyncMessage { chunkCoord = loaded[i], data = data });
        }
    }

    /// <summary>Client: request all loaded NavMap data from the host (call after NavMap.Initialize).</summary>
    public static void RequestFullSync()
    {
        if (!NetworkClient.isConnected || Helper.IsHost()) return;
        NetworkClient.Send(new ClientNavMapRequestMessage());
    }

    private static void OnNavMapSyncMessage(NavMapSyncMessage msg)
    {
        NavMap.ApplySyncData(msg.chunkCoord, msg.data);
    }

    private static void OnNavMapBlockUpdate(NavMapBlockUpdateMessage msg)
    {
        NavMap.Set(msg.blockCoord, msg.isAir);
    }

    private static void OnClientNavMapRequest(NetworkConnectionToClient conn, ClientNavMapRequestMessage _)
    {
        SendNavMapToClient(conn);
    }
}
