using UnityEngine;

[System.Serializable]
public class ConstructionInfo : DestructableInfo
{
    public ID structureID; 
    public override void OnDestroy(Projectile projectile)
    { 
        base.OnDestroy(projectile);
        Entity.Spawn(structureID, Vector3Int.FloorToInt(Machine.transform.position));
    }
}