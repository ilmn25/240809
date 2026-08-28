using System.Collections.Generic;

/// <summary>Data-driven ammo system. Defines which items are ammo and the
/// projectile each one produces when fired, plus which guns accept which ammo
/// (in fire-priority order). A gun fires whatever accepted ammo the shooter is
/// carrying, so any gun can use any bullet (and the shotgun fires shotgun
/// rounds). Shared projectile infos are safe to reuse — per-shot state lives on
/// the Projectile component.</summary>
public static class AmmoRegistry
{
    /// <summary>Regular guns accept any bullet, firing the stronger incendiary rounds first.</summary>
    private static readonly ID[] RegularBullets = { ID.IncendiaryBullet, ID.Bullet };

    private static readonly Dictionary<ID, ProjectileInfo> Projectiles = new()
    {
        { ID.Bullet, new RangedProjectileInfo {
            Sprite = ID.BulletProjectile,
            Damage = 5, Knockback = 6, CritChance = 10,
            LifeSpan = 10000, Speed = 60, Radius = 0.1f, Penetration = 1,
        } },
        { ID.IncendiaryBullet, new FlameProjectile {
            Sprite = ID.BulletProjectile,
            Damage = 2, Knockback = 5, CritChance = 10,
            LifeSpan = 10000, Speed = 60, Radius = 0.1f, Penetration = 1,
        } },
        { ID.ShotgunRound, new ShotgunProjectileInfo {
            Sprite = ID.ShotgunRound,
            Pellets = 6, SpreadAngle = 10f,
            Pellet = new RangedProjectileInfo {
                Sprite = ID.BulletProjectile,
                Damage = 3, Knockback = 4, CritChance = 5,
                LifeSpan = 10000, Speed = 60, Radius = 0.1f,
            },
        } },
    };

    /// <summary>Accepted ammo per gun, in fire-priority order (first carried ammo is fired).</summary>
    private static readonly Dictionary<ID, ID[]> AcceptedByGun = new()
    {
        { ID.Minigun, RegularBullets },
        { ID.Pistol, RegularBullets },
        { ID.FlareGun, RegularBullets },
        { ID.Shotgun, new[] { ID.ShotgunRound } },
    };

    public static bool IsGun(ID id) => AcceptedByGun.ContainsKey(id);
    public static ProjectileInfo GetProjectile(ID ammo) => Projectiles.GetValueOrDefault(ammo);

    /// <summary>Pick the first ammo the shooter actually carries for the given gun.</summary>
    public static ID PickFor(ID gun, Storage storage)
    {
        if (!AcceptedByGun.TryGetValue(gun, out ID[] ammoList)) return ID.Null;
        foreach (ID ammo in ammoList)
            if (storage.GetAmount(ammo) > 0) return ammo;
        return ID.Null;
    }

    /// <summary>A gun's accepted ammo as a display string, e.g. "incendiary bullet, bullet".</summary>
    public static string DescribeForGun(ID gun)
    {
        if (!AcceptedByGun.TryGetValue(gun, out ID[] ammoList) || ammoList.Length == 0) return null;
        string[] names = new string[ammoList.Length];
        for (int i = 0; i < ammoList.Length; i++)
            names[i] = Helper.ToDisplayName(ammoList[i], lowercase: true);
        return string.Join(", ", names);
    }
}
