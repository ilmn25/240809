using UnityEngine;
using Random = UnityEngine.Random;

public class MercenaryTentMachine : StructureMachine
{
    private const int CheckInterval = 200;
    private const int RespawnDelay = 900;

    private Info _mercenaryInfo;
    private int _timer;
    private int _respawnTimer;

    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = 120,
            Loot = ID.MercenaryTent,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Cutting,
            threshold = 1,
            SpawnsRubble = false,
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        _timer = Random.Range(0, CheckInterval);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (++_timer < CheckInterval) return;
        _timer = 0;

        if (MercenaryAlive()) return;

        if (_respawnTimer > 0)
        {
            _respawnTimer--;
            return;
        }

        Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(1, 2, 0);
        _mercenaryInfo = Entity.Spawn(ID.Mercenary, spawnPos);
        if (_mercenaryInfo?.Machine is MercenaryMachine merc)
            merc.Tent = this;
        _respawnTimer = RespawnDelay;
    }

    private bool MercenaryAlive()
    {
        return _mercenaryInfo != null && !_mercenaryInfo.Destroyed && _mercenaryInfo.Machine != null;
    }

    public void Consume()
    {
        if (Info.Destroyed) return;
        Info.Destroy();
        Unload();
    }
}
