using Mirror;
using UnityEngine;

/// <summary>Incremental NavMap update for a single block change.</summary>
public struct NavMapBlockUpdateMessage : NetworkMessage
{
    public Vector3Int blockCoord;
    public byte value; // NavMap.Air / NavMap.Door / NavMap.Block
}

public static class NavMapSync
{
    public static void RegisterHandlers()
    {
        NetworkClient.ReplaceHandler<NavMapBlockUpdateMessage>(OnNavMapBlockUpdate, false);
    }

    /// <summary>Host: broadcast incremental single-block NavMap update to all clients.</summary>
    public static void BroadcastBlockUpdate(Vector3Int blockCoord, byte value)
    {
        if (!NetworkServer.active) return;
        NetworkServer.SendToAll(new NavMapBlockUpdateMessage
        {
            blockCoord = blockCoord,
            value = value
        });
    }

    private static void OnNavMapBlockUpdate(NavMapBlockUpdateMessage msg)
    {
        NavMap.Set(msg.blockCoord, msg.value);
    }
}
