using System.Collections.Generic;
using UnityEngine;

public enum HitboxType {Friendly, Enemy, Passive, All,
    Player
}
public class Projectile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public ProjectileInfo Info;
    public int LifeSpan; 
    public HitboxType TargetHitBoxType;
    public Vector3 Destination;
    public Vector3 Direction;
    public Vector3 Velocity;
    public MobInfo SourceInfo;
    public string Modifier;

    private void Update()
    {
        Info.AI(this);
        LifeSpan++;
    }
    public void Delete()
    {
        LifeSpan = 0;
        ObjectPool.ReturnObject(gameObject);
    } 

    public int GetDamage(out bool crit)
    {
        ItemAffix affix = ItemAffixes.Get(Modifier);
        crit = Random.value * 100f < Info.CritChance + (affix?.Crit ?? 0f);
        int damage = Mathf.RoundToInt(Info.Damage * (affix?.Damage ?? 1f));
        return crit ? damage * 2 : damage;
    }

    public float GetKnockback() => Info.Knockback * (ItemAffixes.Get(Modifier)?.Knockback ?? 1f);

    public static void Spawn(Vector3 origin, Vector3 dest, ProjectileInfo info, HitboxType target, MobInfo sourceInfo, string modifier = null)
    {
        GameObject gameObject = ObjectPool.GetObject(ID.ProjectilePrefab);
        Projectile projectile = gameObject.GetComponent<Projectile>();
        if (!projectile.spriteRenderer)
            projectile.spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        projectile.SourceInfo = sourceInfo;
        projectile.Modifier = modifier;
        projectile.transform.position = origin; 
        projectile.Info = info; 
        projectile.TargetHitBoxType = target; 
        projectile.Destination = dest;
        projectile.Direction = (dest - origin).normalized;
        gameObject.transform.localScale = Vector3.one * info.Scale;
        
        Vector3 direction =  projectile.Direction;
        direction.y = 0;
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        projectile.transform.rotation = rotation * Quaternion.Euler(90, 45, 0); 

        projectile.spriteRenderer.sprite = Cache.LoadSprite("Sprite/" + info.Sprite);
        projectile.Info.OnSpawn(projectile); 
    }
 
} 