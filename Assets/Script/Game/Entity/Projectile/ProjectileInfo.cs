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
    public string Sprite;
    public string Ammo;
    public float Scale = 1;

    public virtual void AI(Projectile projectile) { }
    public virtual void OnSpawn(Projectile projectile) { }
    public int GetDamage()
    {
        return  Random.value < CritChance / 100 ? Damage * 2 : Damage;
    }
 
}