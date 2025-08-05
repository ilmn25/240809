using UnityEngine;
using Random = UnityEngine.Random;

public enum ProjectileClass {Melee, Ranged, Magic, Summon}

public class ProjectileInfo 
{ 
    public float Damage;
    public float Knockback;
    public float CritChance;
    public string Effects;
    public ProjectileClass CLass; 
    public Sprite Sprite = null;

    public virtual void AI(Projectile projectile) { }
    public virtual void OnSpawn(Projectile projectile) { }

    public float GetDamage()
    {
        return  Random.value < CritChance / 100f ? Damage * 2f : Damage;
    }
 
}