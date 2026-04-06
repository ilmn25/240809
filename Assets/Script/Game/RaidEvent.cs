using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public static class RaidEvent
{
    private const float SpawnDistance = 10f;
    private const float PrepareTime = 30f;
    private const float SpawnInterval = 2f;

    private static CoroutineTask _activeRaidTask;

    private sealed class RaidType
    {
        public readonly ID EnemyID;
        public readonly string Message;
        public readonly int MinCount;
        public readonly int MaxCount;

        public RaidType(ID enemyID, string message, int minCount, int maxCount)
        {
            EnemyID = enemyID;
            Message = message;
            MinCount = minCount;
            MaxCount = maxCount;
        }

        public int RollCount() => MinCount == MaxCount ? MinCount : Random.Range(MinCount, MaxCount + 1);
    }

    private static readonly RaidType[] RaidTypes = new RaidType[]
    {
        new(ID.SnareFlea, "You hear skittering in the dark...", 2, 2),
        new(ID.Harpy, "Wings beat overhead...", 2, 2),
        new(ID.SnareFlea, "The swarm is coming...", 3, 4),
        new(ID.Harpy, "Something is circling above...", 4, 6),
        new(ID.Megumin, "A terrible presence gathers...", 5, 7),
        new(ID.Megumin, "A great dread is nearly here...", 7, 10),
    };

    public static void Subscribe() => Event.HourlyTriggered += OnHour;

    private static void OnHour(int hour, int day)
    {
        if (hour != 19 || day % 3 != 0) return;

        _activeRaidTask?.Stop();
        _activeRaidTask = new CoroutineTask(ExecuteRaid(GetRaidType(day)));
    }

    private static IEnumerator ExecuteRaid(RaidType raid)
    {
        ShowRaidWarning(raid.Message);
        yield return new WaitForSeconds(PrepareTime);

        for (int i = 0, count = raid.RollCount(); i < count; i++)
        {
            SpawnRaidEnemy(raid.EnemyID);
            yield return new WaitForSeconds(SpawnInterval);
        }
    }

    private static RaidType GetRaidType(int day)
    {
        if (day <= 8) return RaidTypes[0];
        if (day <= 10) return RaidTypes[1];
        if (day <= 25) return RaidTypes[2];
        if (day <= 50) return RaidTypes[3];
        if (day <= 100) return RaidTypes[4];
        return RaidTypes[5];
    }

    private static void SpawnRaidEnemy(ID enemyID)
    {
        if (Main.PlayerInfo == null || World.Inst.target.Count == 0) return;

        Vector3Int playerPos = Vector3Int.FloorToInt(Main.PlayerInfo.position);
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * SpawnDistance,
            5f,
            Mathf.Sin(angle) * SpawnDistance
        );

        Vector3Int spawnPos = playerPos + Vector3Int.FloorToInt(offset);
        var spawnedEntity = Entity.Spawn(enemyID, spawnPos);

        if (spawnedEntity is MobInfo mobInfo && World.Inst.target.Count > 0)
        {
            mobInfo.Target = World.Inst.target[Random.Range(0, World.Inst.target.Count)];
        }
    }

    private static void ShowRaidWarning(string message)
    {
        Dialogue.Target = new Dialogue { Text = message };
        Dialogue.Show(true);
        Audio.PlaySFX(SfxID.Notification);
    }
}
