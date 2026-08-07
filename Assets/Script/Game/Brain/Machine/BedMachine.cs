/// <summary>A bed that also acts as the Merchant's home, like a pig house. The merchant
/// housing logic lives in MerchantHouse; this bed just drives it and reports its status.</summary>
public class BedMachine : StructureMachine, IActionSecondaryInteract
{
    private MerchantHouse _house;

    public static Info CreateInfo()
    {
        return new Info() { Flammable = true };
    }

    public override void OnStart()
    {
        base.OnStart();
        _house = new MerchantHouse(this);
        _house.OnStart();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        _house.OnUpdate();
    }

    /// <summary>Whether this bed currently has a live merchant.</summary>
    public bool HasMerchant => _house.HasMerchant;

    public void OnActionSecondary(Info info)
    {
        // The bed reports its merchant-house status.
        if (Dialogue.Showing) return;
        Dialogue.Target = new Dialogue { Text = _house.Diagnose() };
        Dialogue.Show(true);
        Audio.PlaySFX(SfxID.Notification);
    }
}