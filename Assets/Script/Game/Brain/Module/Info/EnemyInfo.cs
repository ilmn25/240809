using System;
using UnityEngine;

[System.Serializable]
public class EnemyInfo : MobInfo
{
    private const float CoinDropChance = 0.5f;
    private const float CoinValueMinMultiplier = 0.5f;
    private const float CoinValueMaxMultiplier = 2f;

    /// <summary>Raised when an enemy dies, with the killed enemy's id.</summary>
    public static event System.Action<ID> Killed;

    public override void Initialize()
    {
        base.Initialize();
        Health = HealthMax;
        HitboxType = HitboxType.Enemy;
        targetHitboxType = HitboxType.Player; 
        ActionType = IActionType.Hit;
    }

    protected override void OnUpdate()
    { 
        base.OnUpdate(); 
        FaceTarget = Target != null;
        SpeedTarget = IsGrounded? SpeedGround : SpeedAir; 
        SpeedTarget *= EffectiveSpeedModifier;
        if (Health <= 0)
        { 
            ID id = ((EntityMachine)Machine).Info.id;
            Vector3 position = Machine.transform.position;
            Killed?.Invoke(id);
            Loot.Gettable(id).Spawn(position);
            DropCoins(position);
            Destroy();
            Audio.PlaySFX(DeathSfx);
        }
    }

    private void DropCoins(Vector3 position)
    {
        if (UnityEngine.Random.value > CoinDropChance) return;

        int copper = Mathf.Max(1, (int)(HealthMax * UnityEngine.Random.Range(CoinValueMinMultiplier, CoinValueMaxMultiplier)));
        Trade.SpawnCoins(position, copper);
    }

    protected override void OnHit(Projectile projectile)
    {
        Retaliate(projectile);
    }
 
}