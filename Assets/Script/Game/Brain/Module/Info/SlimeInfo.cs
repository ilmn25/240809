using UnityEngine;

/// <summary>Splits into a babyslime when killed.</summary>
[System.Serializable]
public class SlimeInfo : EnemyInfo
{
    /// <summary>Guaranteed number of babyslimes a slime splits into on death.</summary>
    private const int BabyCount = 2;

    [System.NonSerialized] private bool _spawnedBaby;

    protected override void OnUpdate()
    {
        if (Health <= 0 && !_spawnedBaby)
        {
            _spawnedBaby = true;
            Vector3Int position = Vector3Int.FloorToInt(Machine.transform.position);
            for (int i = 0; i < BabyCount; i++)
                Entity.Spawn(ID.BabySlime, position);
        }
        base.OnUpdate();
    }
}
