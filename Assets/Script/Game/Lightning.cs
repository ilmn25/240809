using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>Strikes bolts around the player when the current environment enables
/// lightning. Environment data controls timing, range, protection, and damage.</summary>
public static class Lightning
{
    private const float MinInterval = 20f;
    private const float MaxInterval = 45f;
    private const float StrikeRange = 30f;
    private const float RodRadius = 10f;
    private const float DamageRadius = 3f;
    private const int Damage = 25;

    // Seconds until the next strike; negative when no strike is scheduled.
    private static float _nextStrike = -1f;

    public static void Update()
    {
        if (!Helper.IsHost() || Save.Inst == null) return;
        if (Save.Inst.players.Count == 0) return;
        Environment settings = Environment.Environments[Save.Inst.weather];
        if (!settings.Lightning)
        {
            _nextStrike = -1f;
            return;
        }
        if (_nextStrike < 0f) _nextStrike = Random.Range(MinInterval, MaxInterval);
        _nextStrike -= Time.deltaTime;
        if (_nextStrike > 0f) return;
        _nextStrike = Random.Range(MinInterval, MaxInterval);

        Vector3 player = Save.Inst.players[Random.Range(0, Save.Inst.players.Count)].position;
        Vector3 spot = new Vector3(
            player.x + Random.Range(-StrikeRange, StrikeRange),
            player.y,
            player.z + Random.Range(-StrikeRange, StrikeRange));

        Info rod = EntityScan.FindNearest(spot, RodRadius, i => i.id == ID.LightningRod);
        Strike(rod != null ? rod.position : spot, rod == null, settings);
    }

    private static void Strike(Vector3 position, bool dealDamage, Environment settings)
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
