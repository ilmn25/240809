using UnityEngine;

/// <summary>A spider web — a harvestable that any tool can cut down. When the
/// player steps on it, nearby spiders are alerted and come to investigate.</summary>
public class SpiderWebMachine : HarvestableMachine
{
    private const float WebRadius = 1.2f;      // how close the player must be to "step on" the web
    private const float AlertRadius = 20f;     // how far away spiders get alerted
    private const int CheckInterval = 20;      // frames between checks

    private int _timer;
    private bool _playerOnWeb;

    public static new Info CreateInfo()
    {
        return new SpiderWebInfo
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
            AlertSpiders(Main.PlayerInfo);
        _playerOnWeb = playerOnWeb;
    }

    private bool PlayerOnWeb()
    {
        if (Main.PlayerInfo == null || Main.PlayerInfo.Destroyed) return false;
        return Vector3.Distance(Main.PlayerInfo.position, transform.position) < WebRadius;
    }

    public void AlertSpiders(Info target)
    {
        if (target == null || target.Destroyed) return;
        EntityScan.ForEach(transform.position, AlertRadius,
            i => i.Machine is SpiderMachine s && !s.Info.Destroyed,
            i => ((SpiderMachine)i.Machine).Investigate(target));
    }
}