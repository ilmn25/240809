using Mirror;

/// <summary>Client → Server: request to toggle a lamp by uid.</summary>
public struct LampToggleMessage : NetworkMessage
{
    public string uid;
}

/// <summary>Server → Clients: authoritative lamp light state.</summary>
public struct LampStateMessage : NetworkMessage
{
    public string uid;
    public bool on;
}

/// <summary>Syncs the lamp's on/off light state between host and clients.
/// Static structures are not part of the periodic entity batch, so lamp
/// toggles travel on their own small message. Joining clients get the
/// correct state from the full-save transfer (GlowOn is serialized).</summary>
public static class LampSync
{
    private static bool _registered;

    public static void RegisterHandlers()
    {
        if (_registered) return;
        NetworkServer.ReplaceHandler<LampToggleMessage>(OnServerLampToggle, false);
        NetworkClient.ReplaceHandler<LampStateMessage>(OnClientLampState, false);
        _registered = true;
    }

    public static void UnregisterHandlers()
    {
        if (!_registered) return;
        NetworkServer.UnregisterHandler<LampToggleMessage>();
        NetworkClient.UnregisterHandler<LampStateMessage>();
        _registered = false;
    }

    public static void Clear()
    {
        _registered = false;
    }

    /// <summary>Called by LampMachine right after a local toggle.
    /// Host broadcasts the new state; a client relays the request to the host.</summary>
    public static void Toggle(LampMachine lamp, StructureInfo structureInfo)
    {
        if (NetworkServer.active)
        {
            NetworkServer.SendToAll(new LampStateMessage
            {
                uid = lamp.Info.uid,
                on = structureInfo.GlowOn
            });
        }
        else if (NetworkClient.isConnected)
        {
            NetworkClient.Send(new LampToggleMessage { uid = lamp.Info.uid });
        }
    }

    private static void OnServerLampToggle(NetworkConnectionToClient conn, LampToggleMessage msg)
    {
        if (!NetworkServer.active) return;
        if (!Info.Dictionary.TryGetValue(msg.uid, out Info info)) return;
        if (info.Machine is not LampMachine lamp) return;
        if (info is not StructureInfo structureInfo) return;

        // Flip the authoritative state; the broadcast below updates every client.
        structureInfo.GlowOn = !structureInfo.GlowOn;
        lamp.SetGlow(structureInfo.GlowOn);
        NetworkServer.SendToAll(new LampStateMessage
        {
            uid = msg.uid,
            on = structureInfo.GlowOn
        });
    }

    private static void OnClientLampState(LampStateMessage msg)
    {
        // Host applies toggles locally already — ignore its own broadcast.
        if (Helper.IsHost()) return;
        if (!Info.Dictionary.TryGetValue(msg.uid, out Info info)) return;
        if (info.Machine is not LampMachine lamp) return;
        if (info is not StructureInfo structureInfo) return;

        structureInfo.GlowOn = msg.on;
        lamp.SetGlow(msg.on);
    }
}
