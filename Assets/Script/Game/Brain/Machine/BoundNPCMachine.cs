using UnityEngine;

/// <summary>A bound NPC that can be rescued by right-clicking. On rescue it is
/// converted into a player and added to the save's player list.</summary>
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

        // Bound NPC stays put — it can't move until rescued.
        if (Info.Target != null)
            Info.CancelTarget();

        SetState<MobIdle>();
    }

    // Converts this bound NPC into a controllable player added to the save.
    private void Rescue()
    {
        if (Save.Inst == null) return;

        // Narrative: the player finds a bound person and unties them.
        Dialogue.Target = new Dialogue
        {
            Text = "\"It's a bound person. You untie them and they join your team.\"",
            Sprite = Cache.LoadSprite("Sprite/BoundNPC"),
        };
        Dialogue.Show(true);

        Vector3 pos = transform.position;
        PlayerInfo player = (PlayerInfo)Entity.CreateInfo(ID.Player, pos);
        player.CharSprite = Info.CharSprite;
        Save.Inst.players.Add(player);

        // Spawn the new player's machine and remove this bound NPC.
        Entity.SpawnFromInfo(player, true);
        Info.Destroy();
        Unload();
        Audio.PlaySFX(SfxID.Text);
    }
}
