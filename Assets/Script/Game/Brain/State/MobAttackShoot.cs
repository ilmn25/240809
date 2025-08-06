using UnityEngine;

public class MobAttackShoot : MobAttack
{
    private Animator _animator;
    private Item _item;
    private MobStatusModule _mobStatusModule;
    public override void OnInitialize()
    {
        _animator = Machine.transform.Find("sprite").GetComponent<Animator>();
        _item = Item.GetItem("minigun");
        _mobStatusModule = Machine.GetModule<MobStatusModule>();
    }

    public override void OnEnterState()
    {
        Audio.PlaySFX(_item.Sfx, 0.5f);
        _animator.speed = _item.Speed; 
        _animator.Play("EquipShoot", 0, 0f);  
        
        Vector3 direction = _mobStatusModule.SpriteToolTrack.right;
        if (_mobStatusModule.SpriteToolTrack.lossyScale.x < 0f) 
            direction *= -1;
        direction.y = 0;
        direction.Normalize();
        
        Projectile.Spawn(_mobStatusModule.SpriteToolTrack.position + direction * Inventory.CurrentItemData.HoldoutOffset,
            _mobStatusModule.Target.transform.position + 0.3f * Vector3.up,
            Inventory.CurrentItemData.ProjectileInfo,
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