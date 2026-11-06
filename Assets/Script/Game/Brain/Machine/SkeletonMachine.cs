using UnityEngine;

/// <summary>A skeleton remains — a harvestable that any tool can break apart for
/// bone loot (including the femur weapon).</summary>
public class SkeletonMachine : HarvestableMachine
{
    private const int BodyFindRadius = 3; // how far to look for a free cell to lie in
    private const int LootFindRadius = 1; // how far loot may land from the body

    public static Info CreateInfo()
    {
        return new HarvestableInfo()
        {
            Health = 30,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
        };
    }

    /// <summary>Turn a dead body into plain remains: a normal skeleton entity on the
    /// nearest free cell where it fell, with everything it carried dropped loose beside it.</summary>
    public static void SpawnRemains(Vector3 position, Storage inventory)
    {
        NavMap.TryFindOpenSpot(Vector3Int.FloorToInt(position), BodyFindRadius, out Vector3Int bodyCell);
        Entity.Spawn(ID.Skeleton, bodyCell);

        // The body cell is now semi-solid, so the nearest open spot is a neighbouring
        // cell — never inside the remains.
        NavMap.TryFindOpenSpot(bodyCell, LootFindRadius, out Vector3Int lootCell);
        inventory.DropAll(lootCell, stackOnSpawn: false);
    }
}
