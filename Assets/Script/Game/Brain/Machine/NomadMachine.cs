using UnityEngine;

/// <summary>A friendly nomad of the travelling bandwagon. The bandwagon leader
/// (IsLeader) opens a shop that sells goods for specific resources; the rest just
/// roam the camp and trade a line of dialogue. Like the merchant, nomads never
/// fight — they flee when attacked.</summary>
public class NomadMachine : GroundMobMachine, IActionSecondaryInteract, IShopkeeper
{
    /// <summary>True for the single shopkeeper of a visiting bandwagon.</summary>
    public bool IsLeader;

    private const int LeaveHour = 19; // sunset — the bandwagon travels on

    /// <summary>The leader's shop inventory, shown through the craft UI. Created
    /// fresh per nomad so Pending/crafting state never bleeds between instances.</summary>
    public CraftInfo Shop { get; private set; }

    public static Info CreateInfo()
    {
        return new PassiveInfo()
        {
            HealthMax = 50,
            SpeedGround = 5,
            SpeedAir = 6,
            DistRoam = 3,
            DistAttack = 2,
            DistDisengage = 40,
            IsNPC = true,
            CharSprite = ID.Merchant, // reuse the merchant's look
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle(600));
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobChase());
        AddState(new MobAttackSwing());
        AddState(new EquipSelectState());
        AddState(new ShopState());

        // The nomad carries a sword to defend itself if attacked.
        Info.SetEquipment(new ItemSlot(ID.SteelSword));

        // A CraftInfo whose id resolves to the shared NomadPool of goods for sale.
        Shop = new CraftInfo { id = ID.Nomad };
    }

    public void OnActionSecondary(Info info)
    {
        if (Info.Target != null) return;

        if (IsLeader)
        {
            Dialogue.Target = new Dialogue { Text = "The road is long. Take a look at what we've gathered." };
            Dialogue.Show(true);
            Audio.PlaySFX(SfxID.Notification);
            SetState<ShopState>();
        }
        else
        {
            Dialogue.Target = new Dialogue { Text = "We trade where the winds take us. Safe travels." };
            Dialogue.Show(true);
            Audio.PlaySFX(SfxID.Notification);
        }
    }

    public override void OnUpdate()
    {
        // The bandwagon leaves at sunset; each nomad despawns on its own.
        if (Save.Inst.time / 60 >= LeaveHour)
        {
            Leave();
            return;
        }

        if (!IsCurrentState<DefaultState>()) return;

        // Retaliate against whoever attacked us (PassiveInfo.OnHit set the target).
        if (Info.Target != null)
        {
            if (Vector3.Distance(Info.Target.position, transform.position) < Info.DistAttack)
            {
                Info.AimPosition = Info.Target.position;
                SetState<MobAttackSwing>();
            }
            else if (Vector3.Distance(Info.Target.position, transform.position) > Info.DistDisengage)
            {
                Info.CancelTarget(); // the threat got away — calm down
                SetState<MobIdle>();
            }
            else
                SetState<MobChase>();
            return;
        }

        SetState<MobIdle>(); // lingers in place
    }

    private void Leave()
    {
        Info.Destroy();
        Unload();
    }
}
