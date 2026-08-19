using UnityEngine;

/// <summary>A friendly nomad of the travelling bandwagon. The bandwagon leader
/// (IsLeader) opens a shop that sells goods for specific resources; the rest just
/// roam the camp and trade a line of dialogue. Like the merchant, nomads flee
/// when attacked but fight back if cornered.</summary>
public class NomadMachine : PassiveNPCMachine, IActionSecondaryInteract, IShopkeeper
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

        AddState(new ShopState());

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

        UpdateFlee();
    }

    private void Leave()
    {
        Info.Destroy();
        Unload();
    }
}
