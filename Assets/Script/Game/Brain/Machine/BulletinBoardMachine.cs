using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A wooden bulletin board that acts as the Questmaster's home in the
/// outpost. NPCs aren't saved, so the board respawns the quest-giver whenever it
/// dies or despawns, the same way the owl statue respawns the Guide.</summary>
public class BulletinBoardMachine : StructureMachine
{
    private const int CheckInterval = 200;  // frames between checks (~3.3s at 60 fps)
    private const int RespawnDelay = 900;   // frames before a lost questmaster respawns (~15s)

    private Info _questmasterInfo;
    private int _timer;
    private int _respawnTimer;

    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = 100,
            Loot = ID.BulletinBoard,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
        };
    }

    public override void OnSetup()
    {
        base.OnSetup();
        // No dedicated sprite yet — reuse the Sign.
        SpriteRenderer.sprite = Cache.LoadSprite("Sprite/Sign");
    }

    public override void OnStart()
    {
        base.OnStart();
        _timer = Random.Range(0, CheckInterval); // stagger boards so they don't all fire at once
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (++_timer < CheckInterval) return;
        _timer = 0;

        if (QuestmasterAlive()) return;

        if (_respawnTimer > 0)
        {
            _respawnTimer--;
            return;
        }

        // Spawn a new questmaster beside the board so it drops down and stands next to it.
        Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(1, 2, 0);
        _questmasterInfo = Entity.Spawn(ID.Questmaster, spawnPos);
        _respawnTimer = RespawnDelay;
    }

    private bool QuestmasterAlive()
    {
        return _questmasterInfo != null && !_questmasterInfo.Destroyed && _questmasterInfo.Machine != null;
    }
}
