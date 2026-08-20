using System.Collections;
using UnityEngine;

/// <summary>The travelling bandwagon's wagon. On arrival it spawns a small camp
/// of followers — nomads and a collector — that cluster around it (see
/// PassiveNPCMachine.UpdateCaravanFollow). After a set time the wagon flees and
/// despawns; the followers leave with it.</summary>
public class CaravanMachine : GroundMobMachine
{
    private const int VisitInterval = 3;   // visit every 3rd day
    private const int ArriveHour = 8;      // morning arrival
    private const float SpawnDistance = 10f;
    private const float FleeDistance = 30f;
    private const float FollowerSpread = 2f;
    private const int FollowerCount = 2;
    private const float VisitDuration = 120f; // seconds the wagon camps before fleeing

    private bool _fleeing;

    public static void Subscribe() => Environment.HourlyTriggered += OnHour;

    private static void OnHour(int hour, int day)
    {
        if (Main.PlayerInfo == null || Main.PlayerInfo.Destroyed) return;
        if (hour != ArriveHour || day % VisitInterval != 0) return;

        Entity.Spawn(ID.Caravan, Event.SpawnPointAroundPlayer(SpawnDistance));
        Dialogue.ShowEvent("A band of nomads sets up camp nearby...");
    }

    public static Info CreateInfo()
    {
        return new PassiveInfo()
        {
            HealthMax = 100,
            SpeedGround = 6,
            SpeedAir = 7,
            DistRoam = 0,
            IsNPC = true,
            CharSprite = ID.Merchant, // no dedicated wagon sprite
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobFleeDespawn(FleeDistance));

        Vector3Int here = Vector3Int.FloorToInt(transform.position);

        // Spawn the camp; the first nomad is the shopkeeper, everyone follows the wagon.
        for (int i = 0; i < FollowerCount; i++)
        {
            Info nomadInfo = Entity.Spawn(ID.Nomad, Event.SpawnPointAround(here, FollowerSpread));
            if (nomadInfo?.Machine is NomadMachine nomad)
            {
                nomad.Caravan = this;
                if (i == 0) nomad.IsLeader = true;
            }
        }

        Info collectorInfo = Entity.Spawn(ID.Collector, Event.SpawnPointAround(here, FollowerSpread));
        if (collectorInfo?.Machine is CollectorMachine collector)
            collector.Caravan = this;

        _ = new CoroutineTask(VisitTimer());
    }

    public override void OnUpdate()
    {
        // Fleeing is handled entirely by the MobFleeDespawn state.
    }

    public void StartFlee()
    {
        if (_fleeing) return;
        _fleeing = true;

        Info.Target = Main.PlayerInfo;
        Info.PathingStatus = PathingStatus.Pending;
        SetState<MobFleeDespawn>();
    }

    private IEnumerator VisitTimer()
    {
        yield return new WaitForSeconds(VisitDuration);
        StartFlee();
    }

}
