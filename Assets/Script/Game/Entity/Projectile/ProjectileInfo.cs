using UnityEngine;
using Random = UnityEngine.Random;

public enum ProjectileClass {Melee, Ranged, Magic, Summon}

public class ProjectileInfo 
{ 
    protected static readonly Collider[] HitBuffer = new Collider[50];
    
    public float Damage;
    public float Knockback;
    public float CritChance;
    public float Radius;  
    public float Speed;
    public float Breaking = 0;
    public string Effects;
    public ProjectileClass Class; 
    public string Sprite;

    public virtual void AI(Projectile projectile) { }
    public virtual void OnSpawn(Projectile projectile) { }

    public float GetDamage()
    {
        return  Random.value < CritChance / 100f ? Damage * 2f : Damage;
    }
 
}