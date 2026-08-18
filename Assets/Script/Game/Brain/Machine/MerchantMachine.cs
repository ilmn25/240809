using UnityEngine;

/// <summary>A travelling merchant that lives at the Old Radio. It never fights or flees;
/// interact with it to open its shop (the craft UI with a fixed inventory of goods).</summary>
public class MerchantMachine : GroundMobMachine, IActionSecondaryInteract, IShopkeeper
{
    /// <summary>The merchant's shop inventory, shown through the craft UI. Created fresh
    /// per merchant so Pending/crafting state never bleeds between instances.</summary>
    public CraftInfo Shop { get; private set; }

    public static Info CreateInfo()
    {
        return new PassiveInfo()
        {
            HealthMax = 50,
            SpeedGround = 5,
            SpeedAir = 6,
            DistRoam = 3,
            IsNPC = true,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new MobIdle(600)); // lingers in place longer than the animals
        AddState(new MobRoam());
        AddState(new MobHit());
        AddState(new MobEscape());
        AddState(new MobAttackSwing());
        AddState(new EquipSelectState());

        AddState(new ShopState());

        // The merchant carries a sword to defend itself if attacked.
        Info.SetEquipment(new ItemSlot(ID.SteelSword));

        // A CraftInfo whose id resolves to the shared MerchantPool of goods for sale.
        // All goods are instant-craft (Time==0 tools or structures), so the shop never
        // needs to sync a Pending queue and isn't registered in Info.Dictionary.
        Shop = new CraftInfo { id = ID.Merchant };
    }

    public void OnActionSecondary(Info info)
    {
        if (Info.Target != null) return;
        Dialogue.Target = new Dialogue { Text = "I've been tracking this signal for ages..." };
        Dialogue.Show(true);
        Audio.PlaySFX(SfxID.Notification);
        SetState<ShopState>();
    }

    public override void OnUpdate()
    {
        if (!IsCurrentState<DefaultState>()) return;

        // The merchant is approachable — it never fights, just flees from anything
        // that attacks it, otherwise stays put.
        if (Info.Target != null)
        {
            if (Vector3.Distance(Info.Target.position, transform.position) > Info.DistDisengage)
                Info.CancelTarget(); // the threat got away — calm down
            else
                SetState<MobEscape>(); // run from the attacker
            return;
        }

        SetState<MobIdle>(); // lingers in place (long idle from OnStart)
    }

    public void OnDrawGizmos()
    {
        if (Camera.current == Camera.main)
            GetModule<GroundPathingModule>().DrawGizmos();
    }
}
