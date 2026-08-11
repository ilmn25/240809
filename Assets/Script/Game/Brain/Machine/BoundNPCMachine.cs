using UnityEngine;

/// <summary>A bound NPC that can be rescued by right-clicking. On rescue it is
/// converted into a player and added to the save's player list.</summary>
public class BoundNPCMachine : MobMachine, IActionSecondaryInteract
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
        };
    }

    public override void OnStart()
    {
        AddModule(new GroundMovementModule());
        AddModule(new GroundPathingModule());
        AddModule(new GroundAnimationModule());
        AddModule(new MobSpriteCullModule());
        AddModule(new SpriteOrbitModule());

        AddState(new MobIdle(600));
        AddState(new MobRoam());
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

        if (Random.value > 0.5f)
            SetState<MobRoam>();
        else
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
