using UnityEngine;

/// <summary>A whirring sawblade that rolls back and forth in a straight line,
/// reversing whenever it runs into a wall. It damages anything it touches. Its
/// movement is manual (no pathing/gravity) so it patrols a fixed line at a fixed
/// height, so it isn't pushed by knockback and doesn't get stuck pathing.</summary>
public class SawbladeMachine : MobMachine
{
    private const float MoveSpeed = 4f;
    private const float LookAhead = 0.6f;  // how far ahead to check for a wall
    private const int ContactInterval = 12; // frames between contact-damage checks

    private static readonly ContactDamageProjectileInfo Blade = new ContactDamageProjectileInfo {
        Damage = 4,
        Knockback = 10,
        CritChance = 0.1f,
        Radius = 0.7f,
    };

    private Vector3 _dir;
    private int _contactTimer;

    public static Info CreateInfo()
    {
        return new EnemyInfo()
        {
            HealthMax = 30,
            CharSprite = ID.Sawmill,
            DistDisengage = 20,
        };
    }

    public override void OnStart()
    {
        _dir = Random.value < 0.5f ? Vector3.right : Vector3.forward;
        if (Random.value < 0.5f) _dir = -_dir;
        if (BlockedAhead(_dir)) _dir = -_dir;
        Info.Direction = _dir;
    }

    public override void OnUpdate()
    {
        if (!Helper.IsHost()) return;
        if (Info.Destroyed) return;

        if (BlockedAhead(_dir))
        {
            _dir = -_dir;
            Info.Direction = _dir;
        }

        transform.position += _dir * (MoveSpeed * Time.deltaTime);

        if (++_contactTimer >= ContactInterval)
        {
            _contactTimer = 0;
            Projectile.Spawn(transform.position, transform.position + _dir, Blade, Info.targetHitboxType, Info);
        }
    }

    private bool BlockedAhead(Vector3 dir)
    {
        Vector3 ahead = transform.position + dir * LookAhead;
        int minX = Mathf.FloorToInt(ahead.x - 0.35f);
        int maxX = Mathf.FloorToInt(ahead.x + 0.35f);
        int minZ = Mathf.FloorToInt(ahead.z - 0.25f);
        int maxZ = Mathf.FloorToInt(ahead.z + 0.25f);
        int minY = Mathf.FloorToInt(ahead.y);
        int maxY = Mathf.FloorToInt(ahead.y + 0.7f);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3Int b = new Vector3Int(x, y, z);
                    if (World.IsInWorldBounds(b) && NavMap.Get(b) != NavMap.Air)
                        return true;
                }
            }
        }
        return false;
    }
}
