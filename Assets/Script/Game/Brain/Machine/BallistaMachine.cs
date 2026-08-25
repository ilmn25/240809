using UnityEngine;

/// <summary>A stationary ballista that looses a bolt down its x or z firing lane
/// whenever a player steps across that lane. It doesn't move or path — it's a
/// turret, not a chaser: cross its line of fire and you take a bolt.</summary>
public class BallistaMachine : MobMachine
{
    private const float TriggerRange = 12f;  // how far along a lane the bolt can reach
    private const float AxisWidth = 0.5f;    // how close to the lane the player must be
    private const float FireInterval = 1.5f; // seconds between shots
    private const float BoltRange = 10f;     // how far the bolt travels

    private static readonly FlameProjectile Bolt = new FlameProjectile {
        Sprite = ID.FlameArrow,
        Damage = 3,
        Knockback = 6,
        CritChance = 5,
        LifeSpan = 200,
        Speed = 12f,
        Radius = 0.2f,
        Class = ProjectileClass.Ranged,
    };

    private float _cooldown;

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 60,
            CharSprite = ID.Ballista,
            DistDisengage = 20,
        };
    }

    public override void OnStart()
    {
        AddModule(new SpriteOrbitModule());
    }

    public override void OnUpdate()
    {
        if (!Helper.IsHost()) return;
        if (Info.Destroyed) return;

        _cooldown -= Time.deltaTime;
        if (_cooldown > 0f) return;

        if (Main.PlayerInfo == null || Main.PlayerInfo.Destroyed) return;
        Vector3 rel = Main.PlayerInfo.position - transform.position;
        if (rel.magnitude > TriggerRange) return;

        // Loose a bolt down whichever lane the player has stepped across.
        if (Mathf.Abs(rel.z) <= AxisWidth)
        {
            Fire(new Vector3(Mathf.Sign(rel.x), 0, 0));
            _cooldown = FireInterval;
        }
        else if (Mathf.Abs(rel.x) <= AxisWidth)
        {
            Fire(new Vector3(0, 0, Mathf.Sign(rel.z)));
            _cooldown = FireInterval;
        }
    }

    private void Fire(Vector3 dir)
    {
        Vector3 origin = transform.position + dir + Vector3.up * 0.4f;
        Projectile.Spawn(origin, origin + dir * BoltRange, Bolt, Info.targetHitboxType, Info);
    }
}
