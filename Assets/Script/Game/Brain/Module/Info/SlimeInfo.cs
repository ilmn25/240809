using UnityEngine;

/// <summary>Splits into a babyslime when killed.</summary>
[System.Serializable]
public class SlimeInfo : EnemyInfo
{
    [System.NonSerialized] private bool _spawnedBaby;

    protected override void OnUpdate()
    {
        if (Health <= 0 && !_spawnedBaby)
        {
            _spawnedBaby = true;
            Entity.Spawn(ID.BabySlime, Vector3Int.FloorToInt(Machine.transform.position));
        }
        base.OnUpdate();
    }
}
