using Mirror;
using UnityEngine;

public struct SpawnProjectileMessage : NetworkMessage
{
    public string sourceUid;
    public ID itemId;
    public Vector3 origin;
    public Vector3 aimPosition;
}

public static class ProjectileSync
{
    public static void SpawnProjectile(MobInfo source, Vector3 origin, Vector3 aim,
        ProjectileInfo info, HitboxType target, ID itemId)
    {
        var msg = new SpawnProjectileMessage
        {
            sourceUid = source.uid,
            itemId = itemId,
            origin = origin,
            aimPosition = aim
        };

        if (NetworkServer.active)
        {
            Projectile.Spawn(origin, aim, info, target, source);
            foreach (var kv in NetworkServer.connections)
                if (kv.Value.connectionId != 0)
                    kv.Value.Send(msg);
        }
        else
        {
            NetworkClient.Send(msg);
        }
    }

    public static void RegisterHandlers()
    {
        NetworkServer.ReplaceHandler<SpawnProjectileMessage>(OnServerSpawnProjectile, false);
        NetworkClient.ReplaceHandler<SpawnProjectileMessage>(OnClientSpawnProjectile, false);
    }

    private static void OnServerSpawnProjectile(NetworkConnectionToClient conn, SpawnProjectileMessage msg)
    {
        if (Info.Dictionary.TryGetValue(msg.sourceUid, out Info rawInfo) && rawInfo is MobInfo sourceInfo)
        {
            ProjectileInfo projInfo = Item.GetItem(msg.itemId)?.ProjectileInfo;
            if (projInfo != null)
                Projectile.Spawn(msg.origin, msg.aimPosition, projInfo, sourceInfo.targetHitboxType, sourceInfo);
        }

        foreach (var kv in NetworkServer.connections)
            if (kv.Value.connectionId != 0)
                kv.Value.Send(msg);
    }

    private static void OnClientSpawnProjectile(SpawnProjectileMessage msg)
    {
        if (Helper.IsHost()) return;

        if (EntitySync.InfoMap.TryGetValue(msg.sourceUid, out Info rawInfo) && rawInfo is MobInfo sourceInfo)
        {
            ProjectileInfo projInfo = Item.GetItem(msg.itemId)?.ProjectileInfo;
            if (projInfo != null)
                Projectile.Spawn(msg.origin, msg.aimPosition, projInfo, sourceInfo.targetHitboxType, sourceInfo);
        }
    }
}
