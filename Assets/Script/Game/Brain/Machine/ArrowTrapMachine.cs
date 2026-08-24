using System.Collections;
using UnityEngine;

/// <summary>A placeable trap that periodically fires an arrow straight along the
/// x or y axis toward the nearest player in range.</summary>
public class ArrowTrapMachine : StructureMachine
{
    private const float TriggerRange = 6f;   // player must be within this range to arm the trap
    private const float ArrowRange = 10f;    // how far the arrow travels
    private const float FireInterval = 1.5f; // seconds between shots

    private static readonly RangedProjectileInfo Arrow = new RangedProjectileInfo {
        Sprite = ID.BulletProjectile,
        Damage = 3,
        Knockback = 6,
        CritChance = 5,
        LifeSpan = 200,
        Speed = 12f,
        Radius = 0.2f,
        Class = ProjectileClass.Ranged,
    };

    private readonly MobInfo _source = new MobInfo { HitboxType = HitboxType.Enemy, targetHitboxType = HitboxType.Player };

    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = 60,
            Loot = ID.ArrowTrap,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Cutting,
            threshold = 1,
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(FireInterval);
            if (!Helper.IsHost()) continue;
            if (Info.Destroyed) yield break;
            if (Main.PlayerInfo == null || Main.PlayerInfo.Destroyed) continue;
            if (Vector3.Distance(Main.PlayerInfo.position, transform.position) > TriggerRange) continue;
            FireArrow();
        }
    }

    private void FireArrow()
    {
        Vector3 rel = Main.PlayerInfo.position - transform.position;
        Vector3 dir = Mathf.Abs(rel.x) > Mathf.Abs(rel.y)
            ? new Vector3(Mathf.Sign(rel.x), 0, 0)
            : new Vector3(0, Mathf.Sign(rel.y), 0);
        Vector3 origin = transform.position + dir;
        Projectile.Spawn(origin, origin + dir * ArrowRange, Arrow, HitboxType.Player, _source);
    }
}
