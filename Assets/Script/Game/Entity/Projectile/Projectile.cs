using System.Collections.Generic;
using UnityEngine;

public enum HitboxType {Friendly, Enemy, Passive, All}
public class Projectile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public ProjectileInfo Info;
    public int LifeSpan;
    public HitboxType Target;
    public Vector3 Destination;
    public Vector3 Direction;
    
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
    
    public static void Spawn(Vector3 origin, Vector3 dest, ProjectileInfo info, HitboxType target)
    {
        GameObject gameObject = ObjectPool.GetObject("projectile");
        Projectile projectile = gameObject.GetComponent<Projectile>();
        if (!projectile.spriteRenderer)
            projectile.spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        
        projectile.transform.position = origin;
        projectile.Info = info; 
        projectile.Target = target; 
        projectile.Destination = dest;
        projectile.Direction = (dest - origin).normalized;
        projectile.transform.rotation = ViewPort.CurrentRotation; 
        projectile.spriteRenderer.sprite = info.Sprite;
        projectile.Info.OnSpawn(projectile); 
    }
 
} 