using System;
using UnityEngine;

public class EntityHandler : MonoBehaviour
{
    public ChunkEntityData _entityData;
    private Vector3Int _positionCurrent;
    private Vector3Int _positionPrevious;
    private Boolean _isStatic;
    public ChunkEntityData GetEntityData()
    {
        _entityData.position = new SerializableVector3Int(WorldSingleton.GetBlockCoordinate(transform.position));
        return _entityData;
    }
 
    public void Initialize(ChunkEntityData entityData, Boolean isStatic) { 
        _entityData = entityData; 
        _isStatic = isStatic;
    }
 
    public void WipeEntity()
    {
        if (_isStatic)
            EntityStaticLoadSingleton._entityList[WorldSingleton.GetChunkCoordinate(transform.position)].Item2.Remove(this);
        else 
            EntityDynamicLoadSingleton._entityList.Remove(this);
        
        EntityPoolSingleton.Instance.ReturnObject(gameObject); 
    }
} 