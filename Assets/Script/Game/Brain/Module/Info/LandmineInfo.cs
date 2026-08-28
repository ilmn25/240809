/// <summary>Info for a landmine: a one-shot pressure-plate mine. On top of normal
/// structure behavior, when a blast (environmental damage with no attacker, e.g.
/// another mine or a grenade) destroys an armed mine it detonates too — so a
/// minefield goes up like dominoes. Broken by a tool or creature it drops back
/// as an item as usual.</summary>
[System.Serializable]
public class LandmineInfo : StructureInfo
{
    public override void OnDestroy(MobInfo info)
    {
        // A blast (info == null) destroying an armed mine sets it off too.
        if (info == null && Machine is LandmineMachine mine)
            mine.OnBlastDestroyed();
    }
}
