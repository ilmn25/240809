using System;

/// <summary>Info for the pigeon. Once it's fleeing (Leaving), being hit doesn't knock
/// it into a hit state — it goes straight back to fleeing, so it never drops out of
/// the escape and back to following the player.</summary>
[System.Serializable]
public class PigeonInfo : EnemyInfo
{
    protected override void OnHit(Projectile projectile)
    {
        // Already escaping — getting hit shouldn't interrupt it. Re-enter flee so it
        // stays leaving (mirrors TreeMimic, which flees on hit).
        if (Machine is PigeonMachine pigeon && pigeon.Leaving)
        {
            pigeon.StartFlee();
            return;
        }
        base.OnHit(projectile);
    }
}
