using UnityEngine;

public class BoundNPCMachine : GroundMobMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        PassiveInfo info = PassiveInfo.CreateNpc();
        info.CharSprite = ID.Chito;
        return info;
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

        // Register with the save's player list so the rescued ally persists across
        // save/load (same contract as the console delver — see DelverMachine).
        Save.Inst.players.Add(delver);

        Entity.SpawnFromInfo(delver, false);
        Info.Destroy();
        Unload();
        Audio.PlaySFX(SfxID.Text);
    }
}
