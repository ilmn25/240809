using Mirror;
using UnityEngine;

/// <summary>Incremental NavMap update for a single block change.</summary>
public struct NavMapBlockUpdateMessage : NetworkMessage
{
    public Vector3Int blockCoord;
    public bool isAir;
}

public static class NavMapSync
{
    public static void RegisterHandlers()
    {
        NetworkClient.ReplaceHandler<NavMapBlockUpdateMessage>(OnNavMapBlockUpdate, false);
    }

    /// <summary>Host: broadcast incremental single-block NavMap update to all clients.</summary>
    public static void BroadcastBlockUpdate(Vector3Int blockCoord, bool isAir)
    {
        if (!NetworkServer.active) return;
        NetworkServer.SendToAll(new NavMapBlockUpdateMessage
        {
            blockCoord = blockCoord,
            isAir = isAir
        });
    }

    private static void OnNavMapBlockUpdate(NavMapBlockUpdateMessage msg)
    {
        NavMap.Set(msg.blockCoord, msg.isAir);
    }
}
