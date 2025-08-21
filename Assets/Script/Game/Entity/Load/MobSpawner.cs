using System.Collections.Generic;
using UnityEngine;

public class MobSpawner
{
    public static int timer;

    public static List<ID> List = new List<ID>()
    {
        ID.SnareFlea,
        // ID.Megumin,
        ID.Slime,
        // ID.Chito,
        // ID.Yuuri
    };
    public static void Update()
    {
        timer++;
        if (timer == 12000)
        {
            timer = 0;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3Int position = Vector3Int.FloorToInt(
                Game.PlayerInfo.position + 
                new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * Scene.RenderDistance/2);
            while (!NavMap.Get(position))
                position.y++; 
            Entity.Spawn(List[Random.Range(0, List.Count)], position); 
        }
    }
}