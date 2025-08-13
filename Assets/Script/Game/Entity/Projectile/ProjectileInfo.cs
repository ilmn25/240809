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
    public float Breaking = 0;
    public string Effects;
    public ProjectileClass Class; 
    public string Sprite;

    public virtual void AI(Projectile projectile) { }
    public virtual void OnSpawn(Projectile projectile) { }
    public int GetDamage()
    {
        return  Random.value < CritChance / 100 ? Damage * 2 : Damage;
    }
 
}