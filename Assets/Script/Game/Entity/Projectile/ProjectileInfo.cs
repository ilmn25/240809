using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ProjectileClass {Melee, Ranged, Magic, Summon}

public class ProjectileInfo 
{ 
    protected static readonly Collider[] HitBuffer = new Collider[50];
    
    public int Damage;
    public float Knockback;
    public float CritChance;
    public float Radius;  
    public float Speed;
    public int Breaking = 0;
    public OperationType OperationType = OperationType.None;
    public string Effects;
    public ProjectileClass Class; 
    public ID Sprite;
    public ID Ammo;
    public float Scale = 1;
    /// <summary>Status effect applied to the victim on a successful hit.</summary>
    public StatusEffect HitEffect;

    public virtual void AI(Projectile projectile) { }
    public virtual void OnSpawn(Projectile projectile) { }
    public int GetDamage()
    {
        return  Random.value < CritChance / 100 ? Damage * 2 : Damage;
    }

    /// <summary>Damage entities in a sphere: 0 hits everything in range, 1 hits only the
    /// closest. The attacker's own target also goes through AbstractHit so structures react.</summary>
    protected void HitSphere(Projectile projectile, Vector3 center, float radius, int maxTargets = 0)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(center, radius, HitBuffer, Main.MaskEntity);
        Machine source = projectile.SourceInfo?.Machine;
        Machine closest = null;
        float closestDistSqr = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            IActionPrimary target = HitBuffer[i].GetComponent<IActionPrimary>();
            if (target == null || source == (Machine)target) continue;

            if (maxTargets == 0)
            {
                Hit(projectile, (Machine)target);
                continue;
            }

            float distSqr = (HitBuffer[i].transform.position - projectile.transform.position).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closest = (Machine)target;
            }
        }

        if (closest != null) Hit(projectile, closest);
    }

    /// <summary>Visit each machine in a sphere except the source; the float is squared distance.</summary>
    protected void ForEachMachine(Projectile projectile, Vector3 center, float radius, Action<Machine, float> action)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(center, radius, HitBuffer, Main.MaskEntity);
        Machine source = projectile.SourceInfo?.Machine;

        for (int i = 0; i < hitCount; i++)
        {
            Machine machine = HitBuffer[i].GetComponent<Machine>();
            if (machine == null || machine == source) continue;

            action(machine, (machine.transform.position - projectile.transform.position).sqrMagnitude);
        }
    }

    private static void Hit(Projectile projectile, Machine target)
    {
        Info info = target.GetModule<Info>();
        info.OnHitInternal(projectile);
        if (info == projectile.SourceInfo.Target)
            projectile.SourceInfo.Target.AbstractHit(projectile);
    }
 
}