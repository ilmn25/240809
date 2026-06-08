using Mirror;
using UnityEngine;

/// <summary>Client → Server: player wants to drop an item from inventory.
/// The client has already modified its own storage locally (which triggers
/// StorageSync broadcast). The server only needs to spawn the world entity.</summary>
public struct ClientDropItemMessage : NetworkMessage
{
    public ID itemID;           // what item to spawn
    public int count;           // how many
    public Vector3 position;    // where to spawn
}

public static class DropSync
{
    private static bool _registered;

    public static void RegisterHandlers()
    {
        if (_registered) return;
        NetworkServer.ReplaceHandler<ClientDropItemMessage>(OnServerDropItem, false);
        _registered = true;
    }

    public static void UnregisterHandlers()
    {
        if (!_registered) return;
        NetworkServer.UnregisterHandler<ClientDropItemMessage>();
        _registered = false;
    }

    private static void OnServerDropItem(NetworkConnectionToClient conn, ClientDropItemMessage msg)
    {
        if (!NetworkServer.active) return;
        if (msg.itemID == ID.Null || msg.count <= 0) return;

        // Spawn the item entity on the server — it'll be synced to all clients
        // via EntitySync.SendBatch.
        Entity.SpawnItem(msg.itemID, msg.position, amount: msg.count);
    }
}
