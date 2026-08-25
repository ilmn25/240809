using UnityEngine;

/// <summary>Keeps exactly one Questmaster alive at the outpost. NPCs aren't
/// saved, so this respawns the quest-giver whenever it dies or is unloaded — the
/// same way the Guide respawns from its statue.</summary>
public static class QuestmasterSpawner
{
    private const int CheckInterval = 300;  // frames between checks
    private const float PlayerRadius = 40f; // only respawn when a player is near the outpost
    private const float AdoptRadius = 24f;  // how close an existing questmaster counts

    private static int _timer;
    private static Info _questmaster;
    private static readonly Collider[] ScanBuffer = new Collider[16];

    public static void Update()
    {
        if (!Helper.IsHost()) return;
        if (World.Inst == null) return;
        if (Save.Inst.current != GenType.Abyss) return; // the outpost (and questmaster) live in the Abyss

        if (++_timer < CheckInterval) return;
        _timer = 0;

        Vector3Int spawn = GenOutpost.GetQuestmasterSpawn(World.Inst);
        if (spawn == Vector3Int.zero) return;

        if (QuestmasterAlive(spawn)) return;
        if (!AnyPlayerNear(spawn)) return;

        _questmaster = Entity.Spawn(ID.Questmaster, spawn);
    }

    private static bool AnyPlayerNear(Vector3 pos)
    {
        foreach (PlayerInfo player in Save.Inst.players)
            if (player.Machine != null && Vector3.Distance(player.Machine.transform.position, pos) <= PlayerRadius)
                return true;
        return false;
    }

    private static bool QuestmasterAlive(Vector3Int spawn)
    {
        if (_questmaster != null && !_questmaster.Destroyed && _questmaster.Machine != null)
            return true;

        // Adopt a nearby questmaster (e.g. after a reload) so we don't stack duplicates.
        int count = Physics.OverlapSphereNonAlloc(spawn, AdoptRadius, ScanBuffer, Main.MaskEntity);
        for (int i = 0; i < count; i++)
        {
            if (ScanBuffer[i].TryGetComponent(out QuestmasterMachine qm) && !qm.Info.Destroyed)
            {
                _questmaster = qm.Info;
                return true;
            }
        }
        return false;
    }
}
