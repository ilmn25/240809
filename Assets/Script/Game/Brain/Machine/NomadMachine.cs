using UnityEngine;

/// <summary>A friendly nomad of the travelling bandwagon. The bandwagon leader
/// (IsLeader) opens a shop that sells goods for specific resources; the rest just
/// cluster around the wagon and trade a line of dialogue. When spawned by a
/// caravan the nomad follows the wagon and leaves with it (see
/// PassiveNPCMachine.UpdateCaravanFollow).</summary>
public class NomadMachine : PassiveNPCMachine, IActionSecondaryInteract, IShopkeeper
{
    /// <summary>True for the single shopkeeper of a visiting bandwagon.</summary>
    public bool IsLeader;

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
        // A caravan nomad follows the wagon and leaves when it's gone.
        if (Caravan != null)
        {
            // Don't interrupt an open shop or a hit reaction.
            if (IsCurrentState<ShopState>() || IsCurrentState<MobHit>())
                return;
            UpdateCaravanFollow();
            return;
        }

        UpdateFlee();
    }
}
