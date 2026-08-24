using Mirror;

/// <summary>Server → Clients: authoritative barrel liquid state.</summary>
public struct BarrelStateMessage : NetworkMessage
{
    public string uid;
    public int liquid;
}

/// <summary>Client → Server: relay a barrel's liquid change (host is authoritative).</summary>
public struct BarrelRelayMessage : NetworkMessage
{
    public string uid;
    public int liquid;
}

/// <summary>Syncs a barrel's stored liquid between host and clients. Static
/// structures are not in the periodic entity batch, so barrel fills/empties
/// travel on their own small message. Joining clients get the correct state
/// from the full-save transfer (Liquid is serialized).</summary>
public static class BarrelSync
{
    private static bool _registered;

    public static void RegisterHandlers()
    {
        if (_registered) return;
        NetworkServer.ReplaceHandler<BarrelRelayMessage>(OnServerRelay, false);
        NetworkClient.ReplaceHandler<BarrelStateMessage>(OnClientState, false);
        _registered = true;
    }

    public static void UnregisterHandlers()
    {
        if (!_registered) return;
        NetworkServer.UnregisterHandler<BarrelRelayMessage>();
        NetworkClient.UnregisterHandler<BarrelStateMessage>();
        _registered = false;
    }

    public static void Clear()
    {
        _registered = false;
    }

    /// <summary>Broadcast a barrel's liquid to clients; a client relays its local
    /// change to the host (which then re-broadcasts authoritatively).</summary>
    public static void Send(BarrelMachine barrel, BarrelInfo bi)
    {
        if (NetworkServer.active)
        {
            NetworkServer.SendToAll(new BarrelStateMessage
            {
                uid = barrel.Info.uid,
                liquid = (int)bi.Liquid
            });
        }
        else if (NetworkClient.isConnected)
        {
            NetworkClient.Send(new BarrelRelayMessage
            {
                uid = barrel.Info.uid,
                liquid = (int)bi.Liquid
            });
        }
    }

    private static void OnServerRelay(NetworkConnectionToClient conn, BarrelRelayMessage msg)
    {
        if (!NetworkServer.active) return;
        if (!Info.Dictionary.TryGetValue(msg.uid, out Info info)) return;
        if (info.Machine is not BarrelMachine barrel) return;
        if (info is not BarrelInfo bi) return;
        barrel.ApplyLiquid((LiquidType)msg.liquid);
    }

    private static void OnClientState(BarrelStateMessage msg)
    {
        // Host applies changes locally already — ignore its own broadcast.
        if (Helper.IsHost()) return;
        if (!Info.Dictionary.TryGetValue(msg.uid, out Info info)) return;
        if (info.Machine is not BarrelMachine barrel) return;
        if (info is not BarrelInfo bi) return;
        barrel.ApplyLiquid((LiquidType)msg.liquid);
    }
}
