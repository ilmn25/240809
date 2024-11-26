using System;
using UnityEngine;

public class EntityHandler : MonoBehaviour
{
    public EntityData _entityData;
    private Vector3Int _positionCurrent;
    private Vector3Int _positionPrevious;
    private Boolean _isStatic;
    public EntityData GetEntityData()
    {
        _entityData.Position = new SerializableVector3(WorldStatic.GetBlockCoordinate(transform.position));
        return _entityData;
    }
 
    public void Initialize(EntityData entityData, Boolean isStatic) { 
        _entityData = entityData; 
        _isStatic = isStatic;
    }
 
    public void WipeEntity()
    {
        if (_isStatic)
            EntityLoadStatic._entityList[WorldStatic.GetChunkCoordinate(transform.position)].Item2.Remove(this);
        else 
            EntityLoadDynamic._entityList.Remove(this);
        
        EntityPoolStatic.Instance.ReturnObject(gameObject); 
    }
} 