using UnityEngine;

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

        Entity.Spawn(ID.Visitor, Event.SpawnPointAroundPlayer(SpawnDistance));
        _active = true;
    }

    /// <summary>Called when the visitor leaves — allows a new one to spawn later.</summary>
    public static void MarkLeft() => _active = false;
}
