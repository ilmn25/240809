using UnityEngine;

[System.Serializable]
public class ConstructionInfo : SpriteStructureInfo
{
    public ID structureID; 

    public override string ToString()
    {
        return $"Building {Helper.ToDisplayName(structureID)} | {Health:0.#} Left";
    }

    public override void OnDestroy(MobInfo info)
    { 
        // Remove the construction ghost from its chunk so it doesn't leave a stale
        // map marker when the real structure spawns in its place.
        Vector3Int chunkCoord = World.GetChunkCoordinate(Machine.transform.position);
        World.Inst[chunkCoord].StaticEntity.Remove(this);
        if (World.Inst.Map != null)
        {
            World.Inst.Map.Dirty = true;
            World.Inst.Map.ResetMarkers();
        }

        Entity.Spawn(structureID, Vector3Int.FloorToInt(Machine.transform.position));
        Tutorial.OnAssembled(structureID);
    }
}