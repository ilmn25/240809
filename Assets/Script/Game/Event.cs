using UnityEngine;

public class Event
{
    public virtual void OnHour(int hour)
    {
    }

    public virtual void Update()
    {
    }
}

public class WeatherEvent : Event
{
    private ID _entityID = ID.Slime;

    public override void Update()
    {
        if (Random.Range(0, 100) < 5 && Main.PlayerInfo != null)
        {
            Vector3Int spawnPosition = Vector3Int.FloorToInt(Main.PlayerInfo.position);
            Entity.Spawn(_entityID, spawnPosition);
        }
    }
}

public class Raid : Event
{
    private ID _entityID = ID.Megumin;

    public override void OnHour(int hour)
    {
        if (hour == 19 && SaveData.Inst.day % 3 == 0 && Main.PlayerInfo != null)
        {
            Vector3Int spawnPosition = Vector3Int.FloorToInt(Main.PlayerInfo.position);
            Entity.Spawn(_entityID, spawnPosition);
        }
    }
}