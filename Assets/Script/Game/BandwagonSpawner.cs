using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A friendly bandwagon of nomads that visits every few days. The group
/// arrives mid-morning near the player and travels on at sunset — each nomad
/// despawns itself (see NomadMachine), so nothing here needs to track them. The
/// leader is a shopkeeper who sells goods for specific resources.</summary>
public static class BandwagonSpawner
{
    private const int VisitInterval = 3;   // visit every 3rd day
    private const int ArriveHour = 8;      // morning arrival
    private const float SpawnDistance = 10f;
    private const float FollowerSpread = 2f;
    private const int FollowerCount = 2;

    public static void Subscribe() => Environment.HourlyTriggered += OnHour;

    private static void OnHour(int hour, int day)
    {
        if (Main.PlayerInfo == null || Main.PlayerInfo.Destroyed) return;
        if (hour != ArriveHour || day % VisitInterval != 0) return;

        Vector3Int leaderPos = AroundPoint(Vector3Int.FloorToInt(Main.PlayerInfo.position), SpawnDistance);

        Info leaderInfo = Entity.Spawn(ID.Nomad, leaderPos);
        if (leaderInfo?.Machine is NomadMachine leader)
            leader.IsLeader = true;

        for (int i = 0; i < FollowerCount; i++)
            Entity.Spawn(ID.Nomad, AroundPoint(leaderPos, FollowerSpread));

        Console.Print("A band of nomads sets up camp nearby...");
        Audio.PlaySFX(SfxID.Notification);
    }

    private static Vector3Int AroundPoint(Vector3Int center, float distance)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(angle) * distance, 5f, Mathf.Sin(angle) * distance);
        return center + Vector3Int.FloorToInt(offset);
    }
}
