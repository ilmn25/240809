using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A placeable owl statue that acts as the Guide's home, like a pig house.
/// NPCs aren't saved, so the statue respawns the Guide whenever it dies or despawns.</summary>
public class OwlStatueMachine : StructureMachine
{
    private const int CheckInterval = 200;  // frames between checks (~3.3s at 60 fps)
    private const int RespawnDelay = 900;   // frames before a lost guide respawns (~15s)
    private const float GuideSearchRadius = 40f;

    private static readonly Collider[] GuideScanBuffer = new Collider[8];

    private Info _guideInfo;
    private int _timer;
    private int _respawnTimer;

    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = 100,
            Loot = ID.OwlStatue,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        AddModule(new NightGlowModule());
        _timer = Random.Range(0, CheckInterval); // stagger statues so they don't all fire at once
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (++_timer < CheckInterval) return;
        _timer = 0;

        if (GuideAlive()) return;

        if (_respawnTimer > 0)
        {
            _respawnTimer--;
            return;
        }

        // Spawn a new guide above the statue so it drops down and stands on top.
        Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(0, 2, 0);
        _guideInfo = Entity.Spawn(ID.Guide, spawnPos);
        _respawnTimer = RespawnDelay;
    }

    // True while this statue's guide is still alive. Falls back to adopting any
    // guide already standing nearby (e.g. after a world reload or when placed
    // beside another statue), so one statue never stacks extra guides.
    private bool GuideAlive()
    {
        if (_guideInfo != null && !_guideInfo.Destroyed && _guideInfo.Machine != null)
            return true;

        int count = Physics.OverlapSphereNonAlloc(transform.position, GuideSearchRadius, GuideScanBuffer, Main.MaskEntity);
        for (int i = 0; i < count; i++)
        {
            if (GuideScanBuffer[i].TryGetComponent(out GuideMachine guide) && !guide.Info.Destroyed)
            {
                _guideInfo = guide.Info;
                return true;
            }
        }
        return false;
    }

    // Glows at night and turns off during the day. Runs in Everyone mode so it
    // updates on every client too (Save.Inst.weather is synced via EnvironmentSync).
    private class NightGlowModule : Module
    {
        public NightGlowModule() { updateMode = UpdateMode.Everyone; }

        public override void Update()
        {
            bool night = Save.Inst.weather == EnvironmentType.NightRainy ||
                         Save.Inst.weather == EnvironmentType.NightBright;
            ((StructureMachine)Machine).SetGlow(night);
        }
    }
}
