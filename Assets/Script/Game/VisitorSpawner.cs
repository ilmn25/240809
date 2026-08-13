using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>Spawns a visitor near the end of the night (hour 22, just before
/// sunrise at 23). The visitor appears at a distance from the player and follows
/// them. Only one visitor is active at a time.</summary>
public static class VisitorSpawner
{
    private const int SpawnHour = 22;        // end of night, before sunrise (23)
    private const float SpawnDistance = 12f; // how far from the player it appears

    private static bool _active;

    public static void Subscribe() => Environment.HourlyTriggered += OnHour;

    private static void OnHour(int hour, int day)
    {
        if (hour != SpawnHour || _active) return;
        if (Main.PlayerInfo == null || Main.PlayerInfo.Destroyed) return;

        Vector3Int playerPos = Vector3Int.FloorToInt(Main.PlayerInfo.position);
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * SpawnDistance,
            5f,
            Mathf.Sin(angle) * SpawnDistance
        );

        Vector3Int spawnPos = playerPos + Vector3Int.FloorToInt(offset);
        Entity.Spawn(ID.Visitor, spawnPos);
        _active = true;
    }

    /// <summary>Called when the visitor leaves — allows a new one to spawn later.</summary>
    public static void MarkLeft() => _active = false;
}
