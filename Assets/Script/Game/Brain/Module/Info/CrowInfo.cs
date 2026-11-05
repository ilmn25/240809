/// <summary>Info for the crow. Crows never fight back — any hit sends them flying,
/// so being struck skips the usual retaliation and takes off instead.</summary>
[System.Serializable]
public class CrowInfo : PassiveInfo
{
    protected override void OnHit(Projectile projectile)
    {
        if (Machine is CrowMachine crow)
            crow.StartFlee(projectile.SourceInfo);
    }
}
