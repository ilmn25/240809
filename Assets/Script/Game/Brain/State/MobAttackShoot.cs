using UnityEngine;

public class MobAttackShoot : MobAttack
{
    private Animator _animator;
    private MobStatusModule _mobStatusModule;
    public override void OnInitialize()
    {
        _animator = Machine.transform.Find("sprite").GetComponent<Animator>();
        _mobStatusModule = Machine.GetModule<MobStatusModule>();
    }

    public override void OnEnterState()
    {
        Audio.PlaySFX(_mobStatusModule.Equipment.Sfx, 0.5f);
        _animator.speed = _mobStatusModule.Equipment.Speed; 
        _animator.Play("EquipShoot", 0, 0f);  
        
        Vector3 direction = _mobStatusModule.SpriteToolTrack.right;
        if (_mobStatusModule.SpriteToolTrack.lossyScale.x < 0f) 
            direction *= -1;
        direction.y = 0;
        direction.Normalize();
        
        Projectile.Spawn(_mobStatusModule.SpriteToolTrack.position + direction * _mobStatusModule.Equipment.HoldoutOffset,
            _mobStatusModule.Target.transform.position + 0.3f * Vector3.up,
            _mobStatusModule.Equipment.ProjectileInfo,
            HitboxType.Friendly);
    }
 
    public override void OnUpdateState()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            _animator.speed = 1f;
            _animator.Play("EquipIdle", 0, 0f);
            Parent.SetState<StateEmpty>();
        } 
    }
}