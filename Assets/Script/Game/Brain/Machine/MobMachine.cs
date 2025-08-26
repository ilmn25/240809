using UnityEngine;

public class MobMachine : EntityMachine, IActionPrimaryAttack
{
    public new MobInfo Info => GetModule<MobInfo>();
    public override void OnSetup()
    {
        if (Info.CharSprite == ID.Null) Info.CharSprite = Info.id;
        transform.Find("Sprite").Find("Char").GetComponent<SpriteRenderer>().sprite =
            Cache.LoadSprite("Sprite/" + Info.CharSprite);
    }
    
    public override void Attack()
    {  
        if (Info.Target != null && !Info.Target.IsInRenderRange) 
            Info.Target.AbstractHit(Info);
         
        if (Info.Equipment.Info.ProjectileInfo != null)
        {
            Vector3 direction = Info.SpriteToolTrack.right;
            if (Info.SpriteToolTrack.lossyScale.x < 0f) 
                direction *= -1;
            direction.y = 0;
            direction.Normalize(); 

            Projectile.Spawn(Info.SpriteToolTrack.position + direction * Info.Equipment.Info.ProjectileOffset,
                Info.AimPosition,
                Info.Equipment.Info.ProjectileInfo,
                Info.targetHitboxType, Info);
        } 
        switch (Info.Equipment.Info.Gesture)
        {
            case ItemGesture.Swing:
                SetState<MobAttackSwing>();
                break;
            case ItemGesture.Shoot:
                SetState<MobAttackShoot>();
                break;
        }
    }
}