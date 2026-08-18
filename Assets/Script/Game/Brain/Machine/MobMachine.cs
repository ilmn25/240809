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