using UnityEngine;

/// <summary>A wooden bulletin board that acts as the Questmaster's home in the
/// outpost. NPCs aren't saved, so the board brings the quest-giver back each new
/// day (or the next time the board loads), the same way the owl statue does for
/// the Guide.</summary>
public class BulletinBoardMachine : SpawnerStructureMachine
{
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

    /// <summary>Spawns a new questmaster beside the board so it drops down and stands next to it.</summary>
    protected override Info SpawnUnit(int index)
    {
        Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(1, 2, 0);
        return Entity.Spawn(ID.Questmaster, spawnPos);
    }
}
