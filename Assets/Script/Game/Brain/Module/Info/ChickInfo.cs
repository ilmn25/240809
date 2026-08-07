using UnityEngine;

/// <summary>Info for baby chicks. When a chick is attacked, it alerts nearby
/// hens and roosters, who turn on the attacker.</summary>
[System.Serializable]
public class ChickInfo : PassiveInfo
{
    private static readonly Collider[] RallyScanBuffer = new Collider[16];

    protected override void OnHit(Projectile projectile)
    {
        base.OnHit(projectile);

        // Rally nearby hens and roosters to defend the chick.
        if (projectile.SourceInfo is not PlayerInfo player) return;
        int count = Physics.OverlapSphereNonAlloc(Machine.transform.position, DistAlert, RallyScanBuffer, Main.MaskEntity);
        for (int i = 0; i < count; i++)
        {
            if (RallyScanBuffer[i].TryGetComponent(out HenMachine hen) && hen.Info.Target is not PlayerInfo)
                hen.Chase(player);
            else if (RallyScanBuffer[i].TryGetComponent(out RoosterMachine rooster) && rooster.Info.Target is not PlayerInfo)
                rooster.Chase(player);
        }
    }
}
