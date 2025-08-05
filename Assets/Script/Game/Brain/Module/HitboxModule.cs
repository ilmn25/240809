using UnityEngine;

public class HitboxModule : Module 
{
    public virtual bool OnHitInternal(Projectile projectile) { return false; }
}

public class DestructableModule : HitboxModule
{
    private float _health;
    private string _sfxHit;
    private string _sfxDestroy;
    private string _loot;
    
    public DestructableModule(float health, string loot, string sfxHit, string sfxDestroy = "")
    {
        _health = health;
        _loot = loot;
        _sfxHit = sfxHit;
        _sfxDestroy = sfxDestroy == ""? sfxHit : sfxDestroy;
    }

    public override bool OnHitInternal(Projectile projectile)
    {
        _health -= projectile.Info.GetDamage();
        if (_health <= 0)
        {
            Audio.PlaySFX(_sfxDestroy); 
            ((EntityMachine)Machine).Delete();
            Entity.SpawnItem(_loot, Vector3Int.FloorToInt(Machine.transform.position), count: 3); 
        }
        else
        {
            Audio.PlaySFX(_sfxHit); 
        }
        return true;
    }
}