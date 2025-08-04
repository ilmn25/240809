using System.Collections.Generic;
using UnityEngine;

public enum ProjectileTarget {Ally, Enemy, Passive, All}
public partial class Projectile : MonoBehaviour
{
    public ProjectileInfo Info;
    public int LifeSpan;
    public ProjectileTarget Target;
    public Vector3 Destination;
    private void Update()
    {
        Info.AI(this);
        LifeSpan++;
    }
    
    public void Delete()
    {
        LifeSpan = 0;
        gameObject.SetActive(false);
        ProjectilePool.Add(this);
    }
}

public partial class Projectile 
{
    private static readonly List<Projectile> ProjectilePool = new List<Projectile>();
    private static int _poolSize = 100;
    
    public static void Spawn(Vector3 origin, Vector3 dest, ProjectileInfo info, ProjectileTarget target)
    {
        Projectile projectile = GetObject();
        if (!projectile) return;
        projectile.transform.position = origin;
        projectile.Info = info; 
        projectile.Target = target; 
        projectile.Destination = dest;
    }
    
    public static Projectile GetObject()
    {
        Projectile projectile;
        if (ProjectilePool.Count == 0)
        {
            if (_poolSize > 0)
            {
                _poolSize--; 
                GameObject gameObject = new GameObject("projectile");
                projectile = gameObject.AddComponent<Projectile>();
                return projectile;
            } 
            return null;
        }
        // return one from pool, and remove from the pool list   
        projectile = ProjectilePool[^1];
        ProjectilePool.Remove(projectile); 
        projectile.transform.gameObject.SetActive(true);
        return projectile;
    }
}