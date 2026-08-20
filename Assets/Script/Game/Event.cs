using System;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Event
{
    public static event Action Updated;

    static Event()
    {
        RaidEvent.Subscribe();
        RainEvent.Subscribe();
        LeafEvent.Subscribe();
        VisitorSpawner.Subscribe();
        CaravanMachine.Subscribe();
    }

    /// <summary>Random position a set distance from a center point, shared by
    /// world events (raids, caravans, visitors).</summary>
    public static Vector3Int SpawnPointAround(Vector3Int center, float distance)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(angle) * distance, 5f, Mathf.Sin(angle) * distance);
        return center + Vector3Int.FloorToInt(offset);
    }

    /// <summary>Spawn position a set distance from the current player.</summary>
    public static Vector3Int SpawnPointAroundPlayer(float distance)
    {
        return SpawnPointAround(Vector3Int.FloorToInt(Main.PlayerInfo.position), distance);
    }

    public static void TriggerUpdate()
    {
        Updated?.Invoke();
    }
}
 
 

public static class RainEvent
{
    private const int ToggleChance = 25;

    public static void Subscribe()
    {
        Environment.HourlyTriggered += OnHour;
    }

    private static void OnHour(int hour, int day)
    {
        if (Random.Range(0, 100) >= ToggleChance) return;

        EnvParticles particleType = Environment.Target == EnvironmentType.DaySnow
            ? EnvParticles.Snow
            : EnvParticles.Rain;
        EnvParticle.Set(particleType);
    }
}

public static class LeafEvent
{
    private const int ToggleChance = 30;

    public static void Subscribe()
    {
        Environment.HourlyTriggered += OnHour;
    }

    private static void OnHour(int hour, int day)
    {
        if (Random.Range(0, 100) >= ToggleChance) return;
 
        EnvParticle.Set(EnvParticles.Leaf);
    }
}