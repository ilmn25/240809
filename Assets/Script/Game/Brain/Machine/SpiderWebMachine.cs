using UnityEngine;

/// <summary>A spider web — a harvestable that any tool can cut down. When the
/// player steps on it, nearby spiders are alerted and come to investigate.</summary>
public class SpiderWebMachine : HarvestableMachine
{
    private const float WebRadius = 1.2f;      // how close the player must be to "step on" the web
    private const float AlertRadius = 20f;     // how far away spiders get alerted
    private const int CheckInterval = 20;      // frames between checks

    private static readonly Collider[] ScanBuffer = new Collider[32];

    private int _timer;
    private bool _playerOnWeb;

    public static Info CreateInfo()
    {
        return new HarvestableInfo
        {
            Health = 8,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
        };
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (!Helper.IsHost()) return;
        if (++_timer < CheckInterval) return;
        _timer = 0;

        bool playerOnWeb = PlayerOnWeb();
        if (playerOnWeb && !_playerOnWeb)
            AlertSpiders();
        _playerOnWeb = playerOnWeb;
    }

    private bool PlayerOnWeb()
    {
        if (Main.PlayerInfo == null || Main.PlayerInfo.Destroyed) return false;
        return Vector3.Distance(Main.PlayerInfo.position, transform.position) < WebRadius;
    }

    private void AlertSpiders()
    {
        int hits = Physics.OverlapSphereNonAlloc(transform.position, AlertRadius, ScanBuffer, Main.MaskEntity);
        for (int i = 0; i < hits; i++)
        {
            if (ScanBuffer[i].TryGetComponent(out SpiderMachine spider) && !spider.Info.Destroyed)
                spider.Investigate(Main.PlayerInfo);
        }
    }
}