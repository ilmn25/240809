using System;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Event
{
    public static event Action<int, int> HourlyTriggered;
    public static event Action Updated;

    static Event()
    {
        RaidEvent.Subscribe();
        RainEvent.Subscribe();
        LeafEvent.Subscribe();
        WeatherEvent.Subscribe();
    }

    public static void TriggerHourly(int hour, int day)
    {
        HourlyTriggered?.Invoke(hour, day);
    }

    public static void TriggerUpdate()
    {
        Updated?.Invoke();
    }
}
 

public static class WeatherEvent
{
    private const int SpawnChance = 5;
    private const ID EntityID = ID.Slime;

    public static void Subscribe()
    {
        Event.Updated += OnUpdate;
    }

    private static void OnUpdate()
    {
        if (Random.Range(0, 100) < SpawnChance && Main.PlayerInfo != null)
        {
            Vector3Int spawnPosition = Vector3Int.FloorToInt(Main.PlayerInfo.position);
            Entity.Spawn(EntityID, spawnPosition);
        }
    }
}

public static class RainEvent
{
    private const int ToggleChance = 25;

    public static void Subscribe()
    {
        Event.HourlyTriggered += OnHour;
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
        Event.HourlyTriggered += OnHour;
    }

    private static void OnHour(int hour, int day)
    {
        if (Random.Range(0, 100) >= ToggleChance) return;
 
        EnvParticle.Set(EnvParticles.Leaf);
    }
}