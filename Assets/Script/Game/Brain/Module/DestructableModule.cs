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
        if (projectile.Target != HitboxType.Enemy) return false;
        _health -= projectile.Info.GetDamage();
        if (_health <= 0)
        {
            Audio.PlaySFX(_sfxDestroy); 
            ((EntityMachine)Machine).Delete();
            Loot.Gettable(_loot).Spawn(Machine.transform.position);
        }
        else
        {
            Audio.PlaySFX(_sfxHit); 
        }
        return true;
    }
}