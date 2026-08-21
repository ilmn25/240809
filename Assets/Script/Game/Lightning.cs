using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>Strikes bolts around the player on rainy nights. A lightning rod
/// within RodRadius catches the bolt and shields that area; otherwise the bolt
/// deals damage to everything in its blast radius.</summary>
public static class Lightning
{
    private const float MinInterval = 20f;   // seconds between bolts
    private const float MaxInterval = 45f;
    private const float StrikeRange = 30f;   // max distance a bolt lands from the player
    private const float RodRadius = 10f;      // how close a rod must be to catch a bolt
    private const float DamageRadius = 3f;   // AoE radius around the impact
    private const int Damage = 25;

    private static float _nextStrike;

    public static void Update()
    {
        if (!Helper.IsHost() || Save.Inst == null) return;
        if (Save.Inst.players.Count == 0) return;
        if (Save.Inst.weather != EnvironmentType.NightRainy) return;

        _nextStrike -= Time.deltaTime;
        if (_nextStrike > 0f) return;
        _nextStrike = Random.Range(MinInterval, MaxInterval);

        Vector3 player = Save.Inst.players[Random.Range(0, Save.Inst.players.Count)].position;
        Vector3 spot = new Vector3(
            player.x + Random.Range(-StrikeRange, StrikeRange),
            player.y,
            player.z + Random.Range(-StrikeRange, StrikeRange));

        Info rod = EntityScan.FindNearest(spot, RodRadius, i => i.id == ID.LightningRod);
        Strike(rod != null ? rod.position : spot, rod == null);
    }

    private static void Strike(Vector3 position, bool dealDamage)
    {
        Vector3 impact = new Vector3(position.x, position.y + 0.1f, position.z);
        Particle.Create(impact, Particles.Lightning, false);
        ScreenShake.Shake(60f, 0.2f, 0.3f);
        Audio.PlaySFX(SfxID.Thunder);

        if (!dealDamage) return;

        EntityScan.ForEach(impact, DamageRadius, IsStrikeable, info =>
        {
            if (info is StructureInfo structure)
                structure.ApplyEnvironmentalDamage(Damage);
            else if (info is DynamicInfo mob && mob.Health > 0)
                mob.Health = Mathf.Max(0, mob.Health - Damage);
        });
    }

    private static bool IsStrikeable(Info info)
    {
        return info is StructureInfo || info is DynamicInfo { Health: > 0 };
    }
}
