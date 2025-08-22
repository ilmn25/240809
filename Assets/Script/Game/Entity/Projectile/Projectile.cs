using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
    public MobInfo SourceInfo;
    public DynamicInfo Target;
    public Quaternion RelativeRotation;

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
    
    public static void Spawn(Vector3 origin, Vector3 dest, ProjectileInfo info, HitboxType target, MobInfo sourceInfo)
    {
        GameObject gameObject = ObjectPool.GetObject(ID.ProjectilePrefab);
        Projectile projectile = gameObject.GetComponent<Projectile>();
        if (!projectile.spriteRenderer)
            projectile.spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        projectile.SourceInfo = sourceInfo;
        projectile.transform.position = origin; 
        projectile.Info = info; 
        projectile.TargetHitBoxType = target; 
        projectile.Destination = dest;
        projectile.Direction = (dest - origin).normalized;
        projectile.Target = null; 
        gameObject.transform.localScale = Vector3.one * info.Scale;
        
        Vector3 direction =  projectile.Direction;
        direction.y = 0;
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        projectile.transform.rotation = rotation * Quaternion.Euler(90, 45, 0); 

        projectile.spriteRenderer.sprite = Cache.LoadSprite("Sprite/" + info.Sprite);
        projectile.Info.OnSpawn(projectile); 
    }
 
} 