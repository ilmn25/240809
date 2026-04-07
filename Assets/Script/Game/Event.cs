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