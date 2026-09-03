using UnityEngine;

public class BoundNPCMachine : GroundMobMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return new PassiveInfo()
        {
            HealthMax = 50,
            SpeedGround = 5,
            SpeedAir = 6,
            DistRoam = 3,
            CharSprite = ID.Chito,
            IsNPC = true,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle());
        AddState(new MobHit());
        AddState(new EquipSelectState());
    }

    public void OnActionSecondary(Info info)
    {
        if (Info.Target != null) return;
        Rescue();
    }

    public override void OnUpdate()
    {
        if (!IsCurrentState<DefaultState>()) return;

        if (Info.Target != null)
            Info.CancelTarget();

        SetState<MobIdle>();
    }

    private void Rescue()
    {
        if (Save.Inst == null) return;

        Dialogue.Target = new Dialogue
        {
            Text = "\"It's a bound person. You untie them and they follow you as an ally.\"",
            Sprite = Cache.LoadSprite("Sprite/BoundNPC"),
        };
        Dialogue.Show(true);

        Vector3 pos = transform.position;
        PlayerInfo delver = (PlayerInfo)Entity.CreateInfo(ID.Delver, pos);
        delver.CharSprite = Info.CharSprite;

        Entity.SpawnFromInfo(delver, false);
        Info.Destroy();
        Unload();
        Audio.PlaySFX(SfxID.Text);
    }
}
